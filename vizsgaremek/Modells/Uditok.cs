using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class Uditok
{
    public int Id { get; set; }

    public string Nev { get; set; } = null!;

    public int? Elerheto { get; set; }

    public byte[] Kep { get; set; } = null!;

    public virtual ICollection<Menuk> Menuks { get; set; } = new List<Menuk>();
}
