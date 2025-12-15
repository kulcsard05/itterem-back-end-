namespace vizsgaremek.Modells
{
    public class Uditok
    {
        public int Id { get; set; }
        public string Nev { get; set; } = null!;
        public ICollection<Menuk> menuks { get; set; } = new List<Menuk>();
    }
}
