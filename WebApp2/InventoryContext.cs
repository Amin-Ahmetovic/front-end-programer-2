using Microsoft.EntityFrameworkCore;

public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<Izdelek> Izdelek { get; set; }
    public DbSet<Skladisce> Skladisce { get; set; }
    public DbSet<SkladisceHasIzdelek> SkladisceHasIzdelek { get; set; }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Izdelek>().ToTable("Izdelek");
        modelBuilder.Entity<Skladisce>().ToTable("Skladisce");
        modelBuilder.Entity<SkladisceHasIzdelek>().ToTable("SkladisceHasIzdelek");

        modelBuilder.Entity<SkladisceHasIzdelek>()
            .HasKey(s => new { s.IDIz, s.IDSk });

        modelBuilder.Entity<SkladisceHasIzdelek>()
            .HasOne(s => s.Izdelek)
            .WithMany()
            .HasForeignKey(s => s.IDIz);

        modelBuilder.Entity<SkladisceHasIzdelek>()
            .HasOne(s => s.Skladisce)
            .WithMany()
            .HasForeignKey(s => s.IDSk);
    }
}

public class Izdelek
{
    public int ID { get; set; }
    public required string Naziv { get; set; }
}

public class Skladisce
{
    public int ID { get; set; }
    public required string Naziv { get; set; }
}

public class SkladisceHasIzdelek
{
    public int IDIz { get; set; }
    public int IDSk { get; set; }
    public int Zaloga { get; set; }

    public Izdelek? Izdelek { get; set; }
    public Skladisce? Skladisce { get; set; }
}

