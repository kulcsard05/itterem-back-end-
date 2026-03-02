using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Channels;
using vizsgaremek.DTOs;
using vizsgaremek.Modells;

namespace vizsgaremek.Controllers
{
    namespace BackEndAlap.Services
    {
        public record OrderStatusUpdate(int OrderId, string Status);

        public class OrderSignalService
        {
            private readonly Channel<bool> _channel = Channel.CreateUnbounded<bool>();
            private readonly Channel<OrderStatusUpdate> _statusChannel = Channel.CreateUnbounded<OrderStatusUpdate>();

            public async Task NotifyChange()
            {
                await _channel.Writer.WriteAsync(true);
            }

            public async Task NotifyStatusChange(int orderId, string status)
            {
                await _statusChannel.Writer.WriteAsync(new OrderStatusUpdate(orderId, status));
            }

            public IAsyncEnumerable<bool> WaitUpdates(CancellationToken ct)
            {
                return _channel.Reader.ReadAllAsync(ct);
            }

            public IAsyncEnumerable<OrderStatusUpdate> WaitStatusUpdates(CancellationToken ct)
            {
                return _statusChannel.Reader.ReadAllAsync(ct);
            }
        }

        [Route("api/[controller]")]
        [ApiController]
        public class RendelesekController : ControllerBase
        {
            private readonly OrderSignalService _signalService;

            public RendelesekController(OrderSignalService signalService)
            {
                _signalService = signalService;
            }

            private bool TryGetTokenUserId(out int userId)
            {
                userId = 0;
                var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return int.TryParse(sub, out userId);
            }

            [Authorize(Policy = "Felhasznalo")]
            [HttpGet("sajat")]
            public async Task<IActionResult> GetSajatRendelesek(CancellationToken ct)
            {
                if (!TryGetTokenUserId(out var tokenUserId))
                {
                    return Unauthorized("Nem sikerült azonosítani a felhasználót a token alapján.");
                }

                using (var cx = new BackEndAlapContext())
                {
                    var responseData = await cx.Rendeleseks
                    .Where(r => r.FelhasznaloId == tokenUserId)
                    .Include(r => r.Felhasznalo)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Keszetel)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Udito)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Menu)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Koret)
                    .OrderByDescending(r => r.Id)
                    .Select(r => new
                    {
                        r.Id,
                        r.FelhasznaloId,
                        FelhasznaloNev = r.Felhasznalo.TeljesNev,
                        r.Datum,
                        r.Statusz,
                        r.OsszesAr,
                        RendelesElemeks = r.RendelesElemeks.Select(e => new
                        {
                            e.Id,
                            e.RendelesId,
                            KeszetelNev = e.Keszetel != null ? e.Keszetel.Nev : null,
                            UditoNev = e.Udito != null ? e.Udito.Nev : null,
                            MenuNev = e.Menu != null ? e.Menu.MenuNev : null,
                            KoretNev = e.Koret != null ? e.Koret.Nev : null,
                            e.Mennyiseg
                        }).ToList()
                    })
                    .ToListAsync(ct);

                    return Ok(responseData);
                }
            }

            [Authorize(Policy = "Felhasznalo")]
            [HttpGet("sajat/{id:int}")]
            public async Task<IActionResult> GetSajatRendelesById(int id, CancellationToken ct)
            {
                if (!TryGetTokenUserId(out var tokenUserId))
                {
                    return Unauthorized("Nem sikerült azonosítani a felhasználót a token alapján.");
                }

                using (var cx = new BackEndAlapContext())
                {
                    var response = await cx.Rendeleseks
                    .Where(r => r.Id == id && r.FelhasznaloId == tokenUserId)
                    .Include(r => r.Felhasznalo)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Keszetel)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Udito)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Menu)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Koret)
                    .Select(r => new
                    {
                        r.Id,
                        r.FelhasznaloId,
                        FelhasznaloNev = r.Felhasznalo.TeljesNev,
                        r.Datum,
                        r.Statusz,
                        r.OsszesAr,
                        RendelesElemeks = r.RendelesElemeks.Select(e => new
                        {
                            e.Id,
                            e.RendelesId,
                            KeszetelNev = e.Keszetel != null ? e.Keszetel.Nev : null,
                            UditoNev = e.Udito != null ? e.Udito.Nev : null,
                            MenuNev = e.Menu != null ? e.Menu.MenuNev : null,
                            KoretNev = e.Koret != null ? e.Koret.Nev : null,
                            e.Mennyiseg
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(ct);

                    if (response == null)
                    {
                        return NotFound("A megadott rendelés nem található, vagy nem a sajátod.");
                    }

                    return Ok(response);
                }
            }

            [Authorize(Policy = "Mindenki")]
            [HttpGet("stream")]
            public async Task GetRendelesStream(CancellationToken ct)
            {
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");
                await Response.WriteAsync(": connected\n\n", ct);
                await Response.Body.FlushAsync(ct);

                var lastId = 0;
                using (var cx = new BackEndAlapContext())
                {
                    lastId = await cx.Rendeleseks.MaxAsync(r => (int?)r.Id, ct) ?? 0;
                }

                try
                {
                    await foreach (var signal in _signalService.WaitUpdates(ct))
                    {
                        lastId = await WriteNewRendelesek(Response, ct, lastId);
                    }
                }
                catch (OperationCanceledException) { }
            }

            [HttpGet("status-stream")]
            public async Task GetStatusStream(CancellationToken ct)
            {
                Response.Headers.Add("Content-Type", "text/event-stream");
                Response.Headers.Add("Cache-Control", "no-cache");
                Response.Headers.Add("Connection", "keep-alive");

                try
                {
                    await foreach (var update in _signalService.WaitStatusUpdates(ct))
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            id = update.OrderId,
                            status = update.Status
                        });
                        await Response.WriteAsync($"data: {json}\n\n", ct);
                        await Response.Body.FlushAsync(ct);
                    }
                }
                catch (OperationCanceledException) { }
            }

            private static async Task<int> WriteNewRendelesek(HttpResponse response, CancellationToken ct, int lastId)
            {
                using (var cx = new BackEndAlapContext())
                {
                    var responseData = await cx.Rendeleseks
                    .Where(r => r.Id > lastId)
                    .Include(r => r.Felhasznalo)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Keszetel)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Udito)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Menu)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Koret)
                    .OrderBy(r => r.Id)
                    .Select(r => new
                    {
                        r.Id,
                        r.FelhasznaloId,
                        r.OsszesAr,
                        RendelesElemeks = r.RendelesElemeks.Select(e => new
                        {
                            e.Id,
                            e.RendelesId,
                            KeszetelNev = e.Keszetel != null ? e.Keszetel.Nev : null,
                            UditoNev = e.Udito != null ? e.Udito.Nev : null,
                            MenuNev = e.Menu != null ? e.Menu.MenuNev : null,
                            KoretNev = e.Koret != null ? e.Koret.Nev : null,
                            e.Mennyiseg
                        }).ToList()
                    })
                    .ToListAsync(ct);

                    if (responseData.Count == 0)
                    {
                        return lastId;
                    }

                    var json = System.Text.Json.JsonSerializer.Serialize(responseData);
                    await response.WriteAsync($"data: {json}\n\n", ct);
                    await response.Body.FlushAsync(ct);

                    return responseData.Max(r => r.Id);
                }
            }

            [Authorize(Policy = "Mindenki")]
            [HttpGet]
            public async Task<IActionResult> GetRendelesek(CancellationToken ct)
            {
                using (var cx = new BackEndAlapContext())
                {
                    var responseData = await cx.Rendeleseks
                    .Include(r => r.Felhasznalo)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Keszetel)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Udito)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Menu)
                    .Include(r => r.RendelesElemeks).ThenInclude(e => e.Koret)

                    .Select(r => new
                    {
                        r.Id,
                        r.FelhasznaloId,
                        FelhasznaloNev = r.Felhasznalo.TeljesNev,
                        r.Datum,
                        r.Statusz,
                        RendelesElemeks = r.RendelesElemeks.Select(e => new
                        {
                            e.Id,
                            e.RendelesId,
                            KeszetelNev = e.Keszetel != null ? e.Keszetel.Nev : null,
                            UditoNev = e.Udito != null ? e.Udito.Nev : null,
                            MenuNev = e.Menu != null ? e.Menu.MenuNev : null,
                            KoretNev = e.Koret != null ? e.Koret.Nev : null,
                            e.Mennyiseg
                        }).ToList()
                    })
                    .ToListAsync(ct);

                    return Ok(responseData);
                }
            }
            [Authorize(Policy = "Felhasznalo")]
            [HttpPost]
            public async Task<IActionResult> Post([FromBody] OrderDto orderDto)
            {
                try
                {
                    using (var cx = new BackEndAlapContext())
                    {
                        if (orderDto == null || orderDto.Items == null || !orderDto.Items.Any())
                        {
                            return BadRequest("A rendelés adatai érvénytelenek vagy hiányoznak.");
                        }

                        var user = await cx.Users.FindAsync(orderDto.FelhasznaloId);
                        if (user == null)
                        {
                            return BadRequest("A megadott felhasználó nem található.");
                        }

                        foreach (var item in orderDto.Items)
                        {
                            int idCount = (item.KeszetelId.HasValue ? 1 : 0) +
                            (item.UditoId.HasValue ? 1 : 0) +
                            (item.MenuId.HasValue ? 1 : 0) +
                            (item.KoretId.HasValue ? 1 : 0);

                            if (idCount != 1)
                            {
                                return BadRequest("Minden tételben pontosan egyet kell megadni az alábbiak közül: készétel, üdítő, menü vagy köret.");
                            }

                            if (item.KeszetelId.HasValue && !await cx.Keszeteleks.AnyAsync(k => k.Id == item.KeszetelId))
                            {
                                return BadRequest($"A(z) {item.KeszetelId} azonosítójú készétel nem található.");
                            }

                            if (item.UditoId.HasValue && !await cx.Uditoks.AnyAsync(u => u.Id == item.UditoId))
                            {
                                return BadRequest($"A(z) {item.UditoId} azonosítójú üdítő nem található.");
                            }

                            if (item.MenuId.HasValue && !await cx.Menuks.AnyAsync(m => m.Id == item.MenuId))
                            {
                                return BadRequest($"A(z) {item.MenuId} azonosítójú menü nem található.");
                            }

                            if (item.KoretId.HasValue && !await cx.Koreteks.AnyAsync(k => k.Id == item.KoretId))
                            {
                                return BadRequest($"A(z) {item.KoretId} azonosítójú köret nem található.");
                            }
                        }

                        Rendelesek targetOrder;
                        var isNewOrder = !orderDto.OrderId.HasValue;

                        if (!isNewOrder)
                        {
                            targetOrder = await cx.Rendeleseks.FindAsync(orderDto.OrderId.Value);
                            if (targetOrder == null)
                            {
                                return BadRequest($"A(z) {orderDto.OrderId.Value} azonosítójú rendelés nem található.");
                            }

                            if (targetOrder.FelhasznaloId != orderDto.FelhasznaloId)
                            {
                                return BadRequest("A megadott rendelés azonosítója nem ehhez a felhasználóhoz tartozik.");
                            }
                        }
                        else
                        {
                            targetOrder = new Rendelesek
                            {
                                FelhasznaloId = orderDto.FelhasznaloId,
                                Datum = DateTime.Now,
                                Statusz = "Függőben",
                                OsszesAr = 0
                            };

                            cx.Rendeleseks.Add(targetOrder);
                        }

                        var itemsTotal = 0;

                        foreach (var item in orderDto.Items)
                        {
                            var qty = Math.Max(1, item.Mennyiseg);

                            int itemPrice;
                            if (item.KeszetelId.HasValue)
                            {
                                itemPrice = await cx.Keszeteleks
                                .Where(k => k.Id == item.KeszetelId.Value)
                                .Select(k => k.Ar)
                                .FirstAsync();
                            }
                            else if (item.UditoId.HasValue)
                            {
                                itemPrice = await cx.Uditoks
                                .Where(u => u.Id == item.UditoId.Value)
                                .Select(u => u.Ar)
                                .FirstAsync();
                            }
                            else if (item.MenuId.HasValue)
                            {
                                itemPrice = await cx.Menuks
                                .Where(m => m.Id == item.MenuId.Value)
                                .Select(m => m.Ar)
                                .FirstAsync();
                            }
                            else
                            {
                                itemPrice = await cx.Koreteks
                                .Where(k => k.Id == item.KoretId.Value)
                                .Select(k => k.Ar)
                                .FirstAsync();
                            }

                            itemsTotal += itemPrice * qty;

                            var orderItem = new RendelesElemek
                            {
                                KeszetelId = item.KeszetelId,
                                UditoId = item.UditoId,
                                MenuId = item.MenuId,
                                KoretId = item.KoretId,
                                Mennyiseg = qty
                            };

                            if (isNewOrder)
                            {
                                orderItem.Rendeles = targetOrder;
                            }
                            else
                            {
                                orderItem.RendelesId = targetOrder.Id;
                            }

                            cx.RendelesElemeks.Add(orderItem);
                        }

                        if (isNewOrder)
                        {
                            targetOrder.OsszesAr = itemsTotal;
                        }
                        else
                        {
                            targetOrder.OsszesAr += itemsTotal;
                        }

                        await cx.SaveChangesAsync();

                        await _signalService.NotifyChange();

                        return Ok(new { OrderId = targetOrder.Id, Message = "Sikeres rendelés." });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Belső hiba történt: {ex.Message}");
                }
            }


            [Authorize(Policy = "Admin")]
            [HttpDelete]

            public IActionResult Delete(int id)
            {
                try
                {
                    using (var cx = new BackEndAlapContext())
                    {
                        var result = cx.Rendeleseks.FirstOrDefault(f => f.Id == id);
                        if (result == null)
                        {
                            return NotFound("A megadott azonosítójú rendelés nem található.");
                        }

                        cx.Rendeleseks.Remove(result);
                        cx.SaveChanges();
                        return Ok("Sikeres törlés.");
                    }
                }
                catch (Exception ex)
                {

                    return BadRequest($"Hiba történt: {ex.Message}");
                }
            }


            [Authorize(Policy = "Admin_Dolgozo")]
            [HttpPut("{id}")]
            public IActionResult ModifyStatus(int id, [FromQuery] string status)
            {
                try
                {
                    using (var cx = new BackEndAlapContext())
                    {
                        var result = cx.Rendeleseks.FirstOrDefault(x => x.Id == id);
                        if (result == null)
                        {
                            return NotFound("Nincs ilyen rendelés.");
                        }

                        if (status == "Függőben" || status == "Folyamatban" || status == "Átvehető" || status == "Átvett")
                        {
                            result.Statusz = status;
                        }
                        else
                        {
                            return BadRequest("Érvénytelen státusz. Elfogadott értékek: Függőben, Folyamatban, Átvehető.");
                        }

                        cx.SaveChanges();

                        _signalService.NotifyStatusChange(result.Id, result.Statusz).GetAwaiter().GetResult();
                        return Ok(result);

                    }
                }
                catch (Exception ex)
                {

                    return BadRequest($"Hiba történt: {ex.Message}");
                }

            }
        }
    }
}
