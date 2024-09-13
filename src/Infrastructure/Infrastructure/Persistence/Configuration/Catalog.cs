
using ECO.WebApi.Domain.Catalog;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ECO.WebApi.Infrastructure.Persistence.Configuration;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .Property(b => b.Name)
                .HasMaxLength(1024);

        builder
            .Property(p => p.MainImage)
                .HasMaxLength(2048);
    }
}
