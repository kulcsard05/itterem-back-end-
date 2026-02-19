namespace vizsgaremek.DTOs
{
    public class LoggedUser
    {
        public string Telefonszam { get; set; }
        public string TeljesNev { get; set; }

        public string Email { get; set; }

        public int Jogosultsag { get; set; }

        public string Token { get; set; }
    }
}
