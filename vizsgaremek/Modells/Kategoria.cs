using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class Kategoria
{
    public int Id { get; set; }

    public string Nev { get; set; } = null!;

    public virtual ICollection<Keszetelek> Keszeteleks { get; set; } = new List<Keszetelek>();
}
