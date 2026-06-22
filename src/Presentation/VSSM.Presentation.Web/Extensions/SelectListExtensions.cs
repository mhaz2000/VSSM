using Microsoft.AspNetCore.Mvc.Rendering;

namespace VSSM.Presentation.Web.Extensions;

public static class SelectListExtensions
{
    public static List<SelectListItem> ToSelectList<T>(
        this IEnumerable<T> items,
        Func<T, object> value,
        Func<T, string> text)
    {
        return items.Select(x => new SelectListItem
        {
            Value = value(x)?.ToString(),
            Text = text(x)
        }).ToList();
    }
}
