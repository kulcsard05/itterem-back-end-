using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class RendelesElemek
{
    public int Id { get; set; }

    public int RendelesId { get; set; }

    public int? KeszetelId { get; set; }

    public int? UditoId { get; set; }

    public int? MenuId { get; set; }

    public int? KoretId { get; set; }

    public int Mennyiseg { get; set; }

    public virtual Keszetelek? Keszetel { get; set; }

    public virtual Koretek? Koret { get; set; }

    public virtual Menuk? Menu { get; set; }

    public virtual Rendelesek Rendeles { get; set; } = null!;

    public virtual Uditok? Udito { get; set; }
}
