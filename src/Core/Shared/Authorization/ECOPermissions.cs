using System.Collections.ObjectModel;
using System.Reflection;

namespace ECO.WebApi.Shared.Authorization;

public static class ECOAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Import = nameof(Import);
    public const string Export = nameof(Export);
    public const string Clean = nameof(Clean);

}

public static class ECOFunction
{
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string User = nameof(User);
    public const string UserRole = nameof(UserRole);
    public const string Role = nameof(Role);
    public const string RoleClaim = nameof(RoleClaim);
    public const string Product = nameof(Product);
    public const string Category = nameof(Category);
}

public record ECOPermission(string action, string function)
{
    public string Name => NameFor(action, function);
    public static string NameFor(string action, string function) => $"Permissions.{function}.{action}";
    public static List<string> GeneratePermissionsForFunction(string function)
    {
        var actions = typeof(ECOAction)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(field => field.IsLiteral && !field.IsInitOnly) // Chỉ lấy các hằng số
        .Select(field => field.GetValue(null)?.ToString())
        .Where(value => value != null) // Loại bỏ các giá trị null
        .ToList();

        // Tạo danh sách quyền động
        return actions.Select(action => $"Permissions.{function}.{action}").ToList();
    }

    public static List<string> GeneratePermissionsForFunction(string function, List<string> actions)
    {
        if (actions == null || actions.Count == 0)
            throw new ArgumentException("Actions list cannot be null or empty", nameof(actions));

        return actions.Select(action => $"Permissions.{function}.{action}").ToList();
    }


}


