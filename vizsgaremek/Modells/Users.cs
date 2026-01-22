namespace vizsgaremek.Modells
{
    public partial class Users
    {
        public int Id { get; set; }
        public int Jogosultsag { get; set; }
        public string TeljesNev { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Hash { get; set; } = null!;
        public string? Salt { get; set; }
        public int Aktiv { get; set; }

        public string TelefonSzam { get; set; } = null!;

        public virtual Jogok? JogosultsagNavigation { get; set; }

    }
}
