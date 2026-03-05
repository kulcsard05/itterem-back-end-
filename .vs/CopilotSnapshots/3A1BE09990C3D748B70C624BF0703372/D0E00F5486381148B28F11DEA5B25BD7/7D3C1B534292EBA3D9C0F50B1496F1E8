using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace vizsgaremek.Hubs
{
    [Authorize]
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var jogosultsag = Context.User?.FindFirst("jogosultsag")?.Value;
            var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
              ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (jogosultsag == "2" || jogosultsag == "3")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "EmployeesGroup");
            }
            else if (userId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnConnectedAsync();
        }
    }
}
