using Microsoft.EntityFrameworkCore;
using faturamento_api.Entities;

namespace faturamento_api.Data;

public class FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> opt) : DbContext(opt)
{
    public DbSet<Nota> Nota => Set<Nota>();
    public DbSet<ItemNota> ItemNota => Set<ItemNota>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasSequence<long>("nota_numeracao_sequence").StartsAt(1).IncrementsBy(1);

        b.Entity<Nota>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero)
                .IsRequired()
                .HasDefaultValueSql("nextval('nota_numeracao_sequence')");
            e.HasIndex(x => x.Numero).IsUnique();
            e.Property(x => x.Status).IsRequired();
            e.ToTable(t => t.HasCheckConstraint("CK_Nota_Status", "\"Status\" IN ('Aberta','Fechada')"));
            e.HasMany(x => x.Itens)
                .WithOne()
                .HasForeignKey(x => x.NotaId)
                .OnDelete(DeleteBehavior.Cascade);;
        });

        b.Entity<ItemNota>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.NotaId, x.ProdutoId });
            e.HasOne<Nota>()
                .WithMany(x => x.Itens)
                .HasForeignKey(x => x.NotaId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.ProdutoId).IsRequired();
            e.Property(x => x.Preco).HasColumnType("numeric(18,2)").IsRequired();
            e.Property(x => x.Quantidade).IsRequired();
            e.ToTable(t => t.HasCheckConstraint("CK_NotaItem_Quantidade_Pos", "\"Quantidade\" > 0"));
        });
    }
}
