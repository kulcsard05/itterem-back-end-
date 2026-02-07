using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class Rendelesek
{
    public int Id { get; set; }

    public int FelhasznaloId { get; set; }

    public DateTime Datum { get; set; }

    public string? Statusz { get; set; }

    public virtual User Felhasznalo { get; set; } = null!;

    public virtual ICollection<RendelesElemek> RendelesElemeks { get; set; } = new List<RendelesElemek>();
}
