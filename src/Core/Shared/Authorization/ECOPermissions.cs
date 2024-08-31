using System.Collections.ObjectModel;

namespace ECO.WebApi.Shared.Authorization;

public static class ECOAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);

}

public static class ECOResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Products = nameof(Products);
    public const string Brands = nameof(Brands);
}

public static class ECOPermissions
{
    private static readonly ECOPermission[] _all = new ECOPermission[]
    {
        new("View Dashboard", ECOAction.View, ECOResource.Dashboard),
        new("View Hangfire", ECOAction.View, ECOResource.Hangfire),
        new("View Users", ECOAction.View, ECOResource.Users),
        new("Search Users", ECOAction.Search, ECOResource.Users),
        new("Create Users", ECOAction.Create, ECOResource.Users),
        new("Update Users", ECOAction.Update, ECOResource.Users),
        new("Delete Users", ECOAction.Delete, ECOResource.Users),
        new("Export Users", ECOAction.Export, ECOResource.Users),
        new("View UserRoles", ECOAction.View, ECOResource.UserRoles),
        new("Update UserRoles", ECOAction.Update, ECOResource.UserRoles),
        new("View Roles", ECOAction.View, ECOResource.Roles),
        new("Create Roles", ECOAction.Create, ECOResource.Roles),
        new("Update Roles", ECOAction.Update, ECOResource.Roles),
        new("Delete Roles", ECOAction.Delete, ECOResource.Roles),
        new("View RoleClaims", ECOAction.View, ECOResource.RoleClaims),
        new("Update RoleClaims", ECOAction.Update, ECOResource.RoleClaims),
        new("View Products", ECOAction.View, ECOResource.Products, IsBasic: true),
        new("Search Products", ECOAction.Search, ECOResource.Products, IsBasic: true),
        new("Create Products", ECOAction.Create, ECOResource.Products),
        new("Update Products", ECOAction.Update, ECOResource.Products),
        new("Delete Products", ECOAction.Delete, ECOResource.Products),
        new("Export Products", ECOAction.Export, ECOResource.Products),
    };

    public static IReadOnlyList<ECOPermission> All { get; } = new ReadOnlyCollection<ECOPermission>(_all);
    public static IReadOnlyList<ECOPermission> Admin { get; } = new ReadOnlyCollection<ECOPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<ECOPermission> Basic { get; } = new ReadOnlyCollection<ECOPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record ECOPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
