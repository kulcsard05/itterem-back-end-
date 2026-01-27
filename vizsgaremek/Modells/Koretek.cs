namespace vizsgaremek.Modells
{
    public class Koretek
    {
        public int Id { get; set; }
        public string Nev { get; set; } = null!;
        public string Leiras { get; set; } = null!;
        public int? elerheto { get; set; }
        public ICollection<Menuk> menuks { get; set; } = new List<Menuk>();
    }
}
