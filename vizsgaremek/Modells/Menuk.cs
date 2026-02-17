using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class Menuk
{
    public int Id { get; set; }

    public string MenuNev { get; set; } = null!;

    public int KeszetelId { get; set; }

    public int KoretId { get; set; }

    public int? UditoId { get; set; }

    public int Elerheto { get; set; }

    public int Ar { get; set; }

    public byte[] Kep { get; set; } = null!;

    public virtual Keszetelek Keszetel { get; set; } = null!;

    public virtual Koretek Koret { get; set; } = null!;

    public virtual ICollection<RendelesElemek> RendelesElemeks { get; set; } = new List<RendelesElemek>();

    public virtual Uditok? Udito { get; set; }
}
