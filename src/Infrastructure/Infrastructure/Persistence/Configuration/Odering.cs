
using ECO.WebApi.Domain.Basket;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ECO.WebApi.Domain.Ordering;
using ECO.WebApi.Domain.Enum;

namespace ECO.WebApi.Infrastructure.Persistence.Configuration;
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder
          .ToTable("Orders", SchemaNames.Ordering);
        builder.Property(x => x.Status).HasDefaultValue(OrderStatus.Pending);

        builder.Property(x => x.CustomerName)
          .HasMaxLength(225)
          .IsRequired();
        builder.Property(x => x.CustomerAddress)
          .HasMaxLength(250)
          .IsRequired();

        builder.Property(x => x.CustomerPhoneNumber)
          .HasMaxLength(50)
          .IsRequired();

        builder.Property(x => x.CustomerEmail)
           .HasMaxLength(50);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder
          .ToTable("OrderItems", SchemaNames.Ordering);
    }
}


public class OrderTransactionConfiguration : IEntityTypeConfiguration<OrderTransaction>
{
    public void Configure(EntityTypeBuilder<OrderTransaction> builder)
    {
        builder
          .ToTable("OrderTransactions", SchemaNames.Ordering);
    }
}

