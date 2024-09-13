
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ECO.WebApi.Domain.Basket;

namespace ECO.WebApi.Infrastructure.Persistence.Configuration;
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder
          .ToTable("Carts", SchemaNames.Basket);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder
          .ToTable("CartItems", SchemaNames.Basket);
    }
}

