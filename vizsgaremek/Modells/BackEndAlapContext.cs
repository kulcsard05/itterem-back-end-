using Microsoft.EntityFrameworkCore;

namespace vizsgaremek.Modells;

public partial class BackEndAlapContext : DbContext
{
    public BackEndAlapContext()
    {
    }

    public BackEndAlapContext(DbContextOptions<BackEndAlapContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Hozzavalok> Hozzavaloks { get; set; }

    public virtual DbSet<Jogok> Jogoks { get; set; }

    public virtual DbSet<Kategoria> Kategoria { get; set; }

    public virtual DbSet<KeszetelHozzavalokKapcsolo> KeszetelHozzavalokKapcsolos { get; set; }

    public virtual DbSet<Keszetelek> Keszeteleks { get; set; }

    public virtual DbSet<Koretek> Koreteks { get; set; }

    public virtual DbSet<Menuk> Menuks { get; set; }

    public virtual DbSet<RendelesElemek> RendelesElemeks { get; set; }

    public virtual DbSet<Rendelesek> Rendeleseks { get; set; }

    public virtual DbSet<Uditok> Uditoks { get; set; }

    public virtual DbSet<User> Users { get; set; }



    readonly string server = Environment.GetEnvironmentVariable("DB_SERVER");
    readonly string db = Environment.GetEnvironmentVariable("DB_NAME");
    readonly string user = Environment.GetEnvironmentVariable("DB_USER");
    readonly string pass = Environment.GetEnvironmentVariable("DB_PASS");
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySQL($"Server={server};Database={db};Uid={user};Pwd={pass}");


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hozzavalok>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("hozzavalok");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.HozzavaloNev)
                .HasMaxLength(50)
                .HasColumnName("hozzavalo_nev");
        });

        modelBuilder.Entity<Jogok>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("jogok");

            entity.HasIndex(e => e.Szint, "szint").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Leiras)
                .HasMaxLength(100)
                .HasColumnName("leiras");
            entity.Property(e => e.Nev)
                .HasMaxLength(64)
                .HasColumnName("nev");
            entity.Property(e => e.Szint)
                .HasColumnType("int(1)")
                .HasColumnName("szint");
        });

        modelBuilder.Entity<Kategoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("kategoria");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Nev)
                .HasMaxLength(60)
                .HasColumnName("nev");
        });

        modelBuilder.Entity<KeszetelHozzavalokKapcsolo>(entity =>
        {
            entity.HasKey(e => new { e.KeszetelId, e.HozzavalokId });

            entity.ToTable("keszetel_hozzavalok_kapcsolo");

            entity.HasIndex(e => e.HozzavalokId, "hozzavalok_id");
            entity.HasIndex(e => e.KeszetelId, "keszetel_id");

            entity.Property(e => e.HozzavalokId)
                .HasColumnType("int(11)")
                .HasColumnName("hozzavalok_id");

            entity.Property(e => e.KeszetelId)
                .HasColumnType("int(11)")
                .HasColumnName("keszetel_id");

            // Make the relationships explicit to avoid EF creating shadow FK columns like 'KeszetelekId'
            entity.HasOne(d => d.Hozzavalok)
                .WithMany() // no navigation on Hozzavalok side
                .HasForeignKey(d => d.HozzavalokId)
                .HasConstraintName("keszetel_hozzavalok_kapcsolo_ibfk_2");

            entity.HasOne(d => d.Keszetel)
                .WithMany(p => p.KeszetelHozzavalokKapcsolos) // match the navigation property added to Keszetelek
                .HasForeignKey(d => d.KeszetelId)
                .OnDelete(DeleteBehavior.Cascade) // match DB if you added CASCADE
                .HasConstraintName("keszetel_hozzavalok_kapcsolo_ibfk_1");
        });

        modelBuilder.Entity<Keszetelek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("keszetelek");

            entity.HasIndex(e => e.KategoriaId, "kategoria_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Ar)
                .HasColumnType("int(11)")
                .HasColumnName("ar");
            entity.Property(e => e.Elerheto)
                .HasColumnType("tinyint(4)")
                .HasColumnName("elerheto");
            entity.Property(e => e.KategoriaId)
                .HasColumnType("int(11)")
                .HasColumnName("kategoria_id");
            entity.Property(e => e.Kep)
                .HasColumnType("mediumblob")
                .HasColumnName("kep");
            entity.Property(e => e.Leiras)
                .HasMaxLength(100)
                .HasColumnName("leiras");
            entity.Property(e => e.Nev)
                .HasMaxLength(64)
                .HasColumnName("nev");

            entity.HasOne(d => d.Kategoria).WithMany(p => p.Keszeteleks)
                .HasForeignKey(d => d.KategoriaId)
                .HasConstraintName("kat");
        });

        modelBuilder.Entity<Koretek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("koretek");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Ar)
                .HasColumnType("int(11)")
                .HasColumnName("ar");
            entity.Property(e => e.Elerheto)
                .HasColumnType("tinyint(4)")
                .HasColumnName("elerheto");
            entity.Property(e => e.Kep)
                .HasColumnType("mediumblob")
                .HasColumnName("kep");
            entity.Property(e => e.Leiras)
                .HasMaxLength(100)
                .HasColumnName("leiras");
            entity.Property(e => e.Nev)
                .HasMaxLength(64)
                .HasColumnName("nev");
        });

        modelBuilder.Entity<Menuk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("menuk");

            entity.HasIndex(e => e.UditoId, "index");

            entity.HasIndex(e => new { e.KeszetelId, e.KoretId }, "keszetel_id");

            entity.HasIndex(e => e.KoretId, "koret_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Ar)
                .HasColumnType("int(11)")
                .HasColumnName("ar");
            entity.Property(e => e.Elerheto)
                .HasColumnType("tinyint(4)")
                .HasColumnName("elerheto");
            entity.Property(e => e.Kep)
                .HasColumnType("mediumblob")
                .HasColumnName("kep");
            entity.Property(e => e.KeszetelId)
                .HasColumnType("int(11)")
                .HasColumnName("keszetel_id");
            entity.Property(e => e.KoretId)
                .HasColumnType("int(11)")
                .HasColumnName("koret_id");
            entity.Property(e => e.MenuNev)
                .HasMaxLength(20)
                .HasColumnName("menu_nev");
            entity.Property(e => e.UditoId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)")
                .HasColumnName("udito_id");

            entity.HasOne(d => d.Keszetel).WithMany(p => p.Menuks)
                .HasForeignKey(d => d.KeszetelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("menuk_ibfk_1");

            entity.HasOne(d => d.Koret).WithMany(p => p.Menuks)
                .HasForeignKey(d => d.KoretId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("menuk_ibfk_2");

            entity.HasOne(d => d.Udito).WithMany(p => p.Menuks)
                .HasForeignKey(d => d.UditoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("udito_menu_kapcs");
        });

        modelBuilder.Entity<RendelesElemek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rendeles_elemek");

            entity.HasIndex(e => e.KeszetelId, "fk_re_keszetel");

            entity.HasIndex(e => e.MenuId, "fk_re_menu");

            entity.HasIndex(e => e.RendelesId, "fk_re_rendeles");

            entity.HasIndex(e => e.UditoId, "fk_re_udito");

            entity.HasIndex(e => e.KoretId, "indexkoret");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.KeszetelId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)")
                .HasColumnName("keszetel_id");
            entity.Property(e => e.KoretId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)")
                .HasColumnName("koret_id");
            entity.Property(e => e.Mennyiseg)
                .HasDefaultValueSql("'1'")
                .HasColumnType("int(11)")
                .HasColumnName("mennyiseg");
            entity.Property(e => e.MenuId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)")
                .HasColumnName("menu_id");
            entity.Property(e => e.RendelesId)
                .HasColumnType("int(11)")
                .HasColumnName("rendeles_id");
            entity.Property(e => e.UditoId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(11)")
                .HasColumnName("udito_id");

            entity.HasOne(d => d.Keszetel).WithMany(p => p.RendelesElemeks)
                .HasForeignKey(d => d.KeszetelId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_re_keszetel");

            entity.HasOne(d => d.Koret).WithMany(p => p.RendelesElemeks)
                .HasForeignKey(d => d.KoretId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("rendeles_elemek_ibfk_1");

            entity.HasOne(d => d.Menu).WithMany(p => p.RendelesElemeks)
                .HasForeignKey(d => d.MenuId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_re_menu");

            entity.HasOne(d => d.Rendeles).WithMany(p => p.RendelesElemeks)
                .HasForeignKey(d => d.RendelesId)
                .HasConstraintName("fk_re_rendeles");

            entity.HasOne(d => d.Udito).WithMany(p => p.RendelesElemeks)
                .HasForeignKey(d => d.UditoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_re_udito");
        });

        modelBuilder.Entity<Rendelesek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rendelesek");

            entity.HasIndex(e => e.FelhasznaloId, "felhasznalo_id");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Datum)
                .HasDefaultValueSql("'current_timestamp()'")
                .HasColumnType("datetime")
                .HasColumnName("datum");
            entity.Property(e => e.FelhasznaloId)
                .HasColumnType("int(11)")
                .HasColumnName("felhasznalo_id");
            entity.Property(e => e.OsszesAr)
                .HasColumnType("int(11)")
                .HasColumnName("osszes_ar");
            entity.Property(e => e.Statusz)
                .HasMaxLength(50)
                .HasDefaultValueSql("'''Függőben'''")
                .HasColumnName("statusz");

            entity.HasOne(d => d.Felhasznalo).WithMany(p => p.Rendeleseks)
                .HasForeignKey(d => d.FelhasznaloId)
                .HasConstraintName("fk_rendeles_user");
        });

        modelBuilder.Entity<Uditok>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("uditok");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Ar)
                .HasColumnType("int(11)")
                .HasColumnName("ar");
            entity.Property(e => e.Elerheto)
                .HasColumnType("tinyint(4)")
                .HasColumnName("elerheto");
            entity.Property(e => e.Kep)
                .HasColumnType("mediumblob")
                .HasColumnName("kep");
            entity.Property(e => e.Nev)
                .HasMaxLength(20)
                .HasColumnName("nev");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => new { e.Email, e.Telefonszam }, "email_2").IsUnique();

            entity.HasIndex(e => e.Jogosultsag, "jogosultsag");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Aktiv)
                .HasColumnType("int(1)")
                .HasColumnName("aktiv");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Hash).HasMaxLength(64);
            entity.Property(e => e.Jogosultsag)
                .HasColumnType("int(1)")
                .HasColumnName("jogosultsag");
            entity.Property(e => e.Salt).HasMaxLength(64);
            entity.Property(e => e.Telefonszam)
                .HasMaxLength(20)
                .HasColumnName("telefonszam");
            entity.Property(e => e.TeljesNev)
                .HasMaxLength(64)
                .HasColumnName("teljes_nev");

            entity.HasOne(d => d.JogosultsagNavigation).WithMany(p => p.Users)
                .HasPrincipalKey(p => p.Szint)
                .HasForeignKey(d => d.Jogosultsag)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
