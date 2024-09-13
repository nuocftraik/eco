
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ECO.WebApi.Domain.Attributes;

namespace ECO.WebApi.Infrastructure.Persistence.Configuration;
public class AttributeConfiguration : IEntityTypeConfiguration<Domain.Attributes.Attribute>
{
    public void Configure(EntityTypeBuilder<Domain.Attributes.Attribute> builder)
    {
        builder
        .ToTable("Attributes", SchemaNames.Attribute);
        builder.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();
    }
}

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder
       .ToTable("AttributeValues", SchemaNames.Attribute);
        builder.Property(x => x.Value)
                .HasMaxLength(50)
                .IsRequired();
    }
}
