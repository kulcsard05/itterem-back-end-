using System;
using System.Collections.Generic;

namespace vizsgaremek.Modells;

public partial class KeszetelHozzavalokKapcsolo
{
    public int KeszetelId { get; set; }

    public int HozzavalokId { get; set; }

    public virtual Hozzavalok Hozzavalok { get; set; } = null!;

    public virtual Keszetelek Keszetel { get; set; } = null!;
}
