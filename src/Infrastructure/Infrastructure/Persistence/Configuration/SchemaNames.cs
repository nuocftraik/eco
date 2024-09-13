
namespace ECO.WebApi.Infrastructure.Persistence.Configuration;
internal static class SchemaNames
{
    // TODO: figure out how to capitalize these only for Oracle
    public static string Auditing = nameof(Auditing); // "AUDITING";
    public static string Catalog = nameof(Catalog); // "CATALOG";
    public static string Basket = nameof(Basket); // "BASKET";
    public static string Ordering = nameof(Ordering); // "ORDERING";
    public static string Identity = nameof(Identity); // "IDENTITY";
}
