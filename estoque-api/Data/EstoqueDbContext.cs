using Microsoft.EntityFrameworkCore;
using estoque_api.Entities;

namespace estoque_api.Data;

public class EstoqueDbContext(DbContextOptions<EstoqueDbContext> opt) : DbContext(opt)
{
    public DbSet<Produto> Produto => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Produto>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property<uint>("xmin")
                .HasColumnName("xmin")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate(); 
        });
    }
}