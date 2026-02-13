using System.Linq.Expressions;
using System.Reflection;

namespace VSSM.Application.Helpers;

public static class QueryHelpler
{
    public static IQueryable<T> ApplySorting<T>(
   this IQueryable<T> query,
   string sortBy, bool descending)
    {
        var propertyName = sortBy.Trim();

        var property = typeof(T).GetProperty(
            propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property == null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(member, parameter);

        var methodName = descending ? "OrderByDescending" : "OrderBy";

        var method = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
    }
}
