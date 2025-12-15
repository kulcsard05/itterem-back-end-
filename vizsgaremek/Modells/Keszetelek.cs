namespace vizsgaremek.Modells
{
    public class Keszetelek
    {
        public int Id { get; set; }
        public string Nev { get; set; } = null!;
        public string Leiras { get; set; } = null!;
        public ICollection<Hozzavalok> Hozzavaloks { get; set; } = new List<Hozzavalok>();
        public ICollection<Menuk> Menuks { get; set; } = new List<Menuk>();
    }
}
