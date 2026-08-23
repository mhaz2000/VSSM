using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace VSSM.Presentation.Web.TagHelpers;

/// <summary>
/// [SECURITY - A05]: injects the per-request CSP nonce (set by SecurityHeadersMiddleware in
/// HttpContext.Items["CspNonce"]) into every &lt;script&gt; and &lt;style&gt; element so inline blocks
/// are allowed by the nonce-based CSP. External scripts/styles are unaffected ('self' covers them).
/// </summary>
[HtmlTargetElement("script")]
[HtmlTargetElement("style")]
public class CspNonceTagHelper : TagHelper
{
    [ViewContext] public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext.HttpContext.Items.TryGetValue("CspNonce", out var raw) && raw is string nonce)
        {
            if (!output.Attributes.ContainsName("nonce"))
                output.Attributes.SetAttribute("nonce", nonce);
        }
    }
}
