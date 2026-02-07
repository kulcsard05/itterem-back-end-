using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class User
{
    public int Id { get; set; }

    public int Jogosultsag { get; set; }

    public string TeljesNev { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telefonszam { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public int Aktiv { get; set; }

    public virtual Jogok JogosultsagNavigation { get; set; } = null!;

    public virtual ICollection<Rendelesek> Rendeleseks { get; set; } = new List<Rendelesek>();
}
