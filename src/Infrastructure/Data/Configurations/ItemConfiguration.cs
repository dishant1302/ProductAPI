using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductAPI.Domain.Entities;

namespace ProductAPI.Infrastructure.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Item");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .UseIdentityColumn();

        builder.Property(i => i.Quantity)
            .IsRequired();

        // Index for FK
        builder.HasIndex(i => i.ProductId);
    }
}