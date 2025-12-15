namespace vizsgaremek.Modells
{
    public class Menuk
    {
        public int Id { get; set; }
        public string Menu_Nev { get; set; } = null!;
        public int Keszetel_Id { get; set; }
        public int Koret_Id { get; set; }
        public int Udito_Id { get; set; }

        // Navigation properties (one-to-one/many-to-one)
        public Keszetelek Keszetelek { get; set; } = null!;
        public Koretek Koretek { get; set; } = null!;
        public Uditok Uditok { get; set; } = null!;
    }
}
