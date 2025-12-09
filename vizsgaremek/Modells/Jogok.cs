namespace vizsgaremek.Modells
{
    public partial class Jogok
    {
        public int Id { get; set; }
        public int Szint { get; set; }
        public string Nev { get; set; } = null!;
        public string Leiras { get; set; } = null!;

        public virtual ICollection<Users> Users { get; set; } = new List<Users>();

    }
}
