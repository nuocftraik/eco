using DocumentFormat.OpenXml.Bibliography;
using ECO.WebApi.Application.Common.Events;
using ECO.WebApi.Application.Common.Interfaces;
using ECO.WebApi.Domain.Attributes;
using ECO.WebApi.Domain.Basket;
using ECO.WebApi.Domain.Catalog;
using ECO.WebApi.Domain.Identity;
using ECO.WebApi.Domain.Notifications;
using ECO.WebApi.Domain.Ordering;
using ECO.WebApi.Domain.Payment;
using ECO.WebApi.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using Action = ECO.WebApi.Domain.Identity.Action;
namespace ECO.WebApi.Infrastructure.Persistence.Context;
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions options, IOptions<DatabaseSettings> dbSettings, ICurrentUser currentUser, ISerializerService serializer, IEventPublisher events)
        : base(options, dbSettings, currentUser, serializer, events)
    {
    }

    //Attribute
    public DbSet<Domain.Attributes.Attribute> Attributes { get; set; }
    public DbSet<AttributeValue> AttributeValues { get; set; }

    //Catalog
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Variant> Variants { get; set; }
    public DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }
    public DbSet<UserReview> UserReviews { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; }
    public DbSet<Domain.Catalog.Tag> Tags { get; set; }

    //Ordering
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    //Basket
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    //Identity
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Function> Functions { get; set; }
    public DbSet<Action> Actions { get; set; }
    public DbSet<ActionInFunction> ActionInFunctions { get; set; }

    //Payment
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Notification> Notifications { get; set; }

}
