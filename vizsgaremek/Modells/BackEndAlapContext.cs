namespace vizsgaremek.Modells;
using Microsoft.EntityFrameworkCore;

public partial class BackEndAlapContext : DbContext
{
    public BackEndAlapContext()
    {
    }

    public BackEndAlapContext(DbContextOptions<BackEndAlapContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Jogok> Jogoks { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    public virtual DbSet<Hozzavalok> Hozzavaloks { get; set; }

    public virtual DbSet<Keszetelek> Keszeteleks { get; set; }
    public virtual DbSet<Menuk> Menuks { get; set; }

    public virtual DbSet<Koretek> Koreteks { get; set; }

    public virtual DbSet<Uditok> Uditoks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("SERVER=localhost;PORT=3306;DATABASE=itterem;USER=root;PASSWORD=;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Jogok>(entity =>
        {

            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("jogok");

            entity.HasIndex(e => e.Szint, "Szint").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Leiras).HasMaxLength(100);
            entity.Property(e => e.Nev).HasMaxLength(64);
            entity.Property(e => e.Szint).HasColumnType("int(1)");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            // Egyedi indexek az email és telefonszam mezőkre
            entity.HasIndex(e => e.Email, "email").IsUnique();
            entity.HasIndex(e => e.TelefonSzam, "telefonszam").IsUnique(); // ← Telefonszám egyedi index
            entity.HasIndex(e => e.Jogosultsag, "jogosultsag");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.TeljesNev).HasMaxLength(64).HasColumnName("teljes_nev");
            entity.Property(e => e.TelefonSzam).HasMaxLength(20).HasColumnName("telefonszam");
            entity.Property(e => e.Hash).HasMaxLength(64).HasColumnName("Hash");
            entity.Property(e => e.Salt).HasMaxLength(64).HasColumnName("Salt");
            entity.Property(e => e.Jogosultsag).HasColumnType("int(1)");
            entity.Property(e => e.Aktiv).HasColumnType("int(1)");

            entity.HasOne(d => d.JogosultsagNavigation)
      .WithMany(p => p.Users)
      .HasPrincipalKey(p => p.Szint)
      .HasForeignKey(d => d.Jogosultsag)
      .OnDelete(DeleteBehavior.ClientSetNull) // Opcionálissá teszi
      .HasConstraintName("users_ibfk_1");
        });

        modelBuilder.Entity<Hozzavalok>(entity =>
        {
            entity.ToTable("hozzavalok");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            // matches SQL: varchar(50)
            entity.Property(e => e.Hozzavalo_Nev).HasMaxLength(50).HasColumnName("hozzavalo_nev");
        });

        modelBuilder.Entity<Keszetelek>(entity =>
        {
            entity.ToTable("keszetelek");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Id).HasColumnType("int(11)");

            entity.Property(e => e.Nev).HasMaxLength(64).HasColumnName("nev");
            entity.Property(e => e.Leiras).HasMaxLength(100).HasColumnName("leiras");

            // ADD THIS:
            entity.Property(e => e.Elerheto).HasColumnType("int(1)").HasColumnName("elerheto");
        });

        // HIÁNYZÓ entitások konfigurációja:
        modelBuilder.Entity<Koretek>(entity =>
        {
            entity.ToTable("koretek");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Nev).HasMaxLength(64);
            entity.Property(e => e.Leiras).HasMaxLength(100);
        });

        modelBuilder.Entity<Uditok>(entity =>
        {
            entity.ToTable("uditok");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Nev).HasMaxLength(20);
        });

        modelBuilder.Entity<Menuk>(entity =>
        {
            entity.ToTable("menuk");
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Menu_Nev).HasMaxLength(20).HasColumnName("menu_nev");
            entity.Property(e => e.Keszetel_Id).HasColumnName("keszetel_id");
            entity.Property(e => e.Koret_Id).HasColumnName("koret_id");
            entity.Property(e => e.Udito_Id).HasColumnName("udito_id");

            // Foreign key relationships
            entity.HasOne(d => d.Keszetelek).WithMany(p => p.Menuks)
                  .HasForeignKey(d => d.Keszetel_Id)
                  .HasConstraintName("menuk_ibfk_1");
            entity.HasOne(d => d.Koretek).WithMany(p => p.menuks)
                  .HasForeignKey(d => d.Koret_Id)
                  .HasConstraintName("menuk_ibfk_2");
            entity.HasOne(d => d.Uditok).WithMany(p => p.menuks)
                  .HasForeignKey(d => d.Udito_Id)
                  .HasConstraintName("udito_menu_kapcs");
        });

        // Map many-to-many to existing join table `keszetel_hozzavalok_kapcsolo`
        modelBuilder.Entity<Hozzavalok>()
            .HasMany(h => h.Keszeteleks)
            .WithMany(k => k.Hozzavaloks)
            .UsingEntity<Dictionary<string, object>>(
                "keszetel_hozzavalok_kapcsolo",
                j => j
                    .HasOne<Keszetelek>()
                    .WithMany()
                    .HasForeignKey("keszetel_id")
                    .HasConstraintName("keszetel_hozzavalok_kapcsolo_ibfk_1")
                    .OnDelete(DeleteBehavior.NoAction),
                j => j
                    .HasOne<Hozzavalok>()
                    .WithMany()
                    .HasForeignKey("hozzavalok_id")
                    .HasConstraintName("keszetel_hozzavalok_kapcsolo_ibfk_2")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("keszetel_id", "hozzavalok_id");
                    j.ToTable("keszetel_hozzavalok_kapcsolo");
                    j.HasIndex(new[] { "keszetel_id" }, "keszetel_id");
                    j.HasIndex(new[] { "hozzavalok_id" }, "hozzavalok_id");
                });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
