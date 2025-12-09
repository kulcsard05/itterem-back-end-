namespace vizsgaremek.Modells;
using Microsoft.EntityFrameworkCore;

public partial class BackEndAlapContext :DbContext
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("SERVER=localhost;PORT=3306;DATABASE=backend_alap;USER=root;PASSWORD=;");

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

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.Email, "Email_2");

            entity.HasIndex(e => e.TeljesNev, "FelhasznaloNev").IsUnique();

            entity.HasIndex(e => e.Jogosultsag, "Jogosultsag");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.TeljesNev).HasMaxLength(100);
            entity.Property(e => e.Hash)
                .HasMaxLength(64)
                .HasColumnName("HASH");
            entity.Property(e => e.Jogosultsag).HasColumnType("int(1)");
            entity.Property(e => e.Salt)
                .HasMaxLength(64)
                .HasColumnName("SALT");
            entity.Property(e => e.TeljesNev).HasMaxLength(60);

            entity.HasOne(d => d.JogosultsagNavigation).WithMany(p => p.Users)
                .HasPrincipalKey(p => p.Szint)
                .HasForeignKey(d => d.Jogosultsag)
                .HasConstraintName("user_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
