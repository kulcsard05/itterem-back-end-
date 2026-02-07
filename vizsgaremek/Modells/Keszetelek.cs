using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class Keszetelek
{
    public int Id { get; set; }

    public string Nev { get; set; } = null!;

    public string Leiras { get; set; } = null!;

    public int Elerheto { get; set; }

    public int KategoriaId { get; set; }

    public byte[] Kep { get; set; } = null!;

    public virtual Kategora Kategoria { get; set; } = null!;

    public virtual ICollection<Menuk> Menuks { get; set; } = new List<Menuk>();

    public virtual ICollection<RendelesElemek> RendelesElemeks { get; set; } = new List<RendelesElemek>();
}
