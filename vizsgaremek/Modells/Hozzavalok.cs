namespace vizsgaremek.Modells
{
    public class Hozzavalok
    {
        public int Id { get; set; }
        public string Hozzavalo_Nev { get; set; } = null!;
        public ICollection<Keszetelek> Keszeteleks { get; set; } = new List<Keszetelek>();
    }
}
