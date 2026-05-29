using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DitibStasbourg.TagHelpers
{
    [HtmlTargetElement("authorized-button", Attributes = "claim")]
    [HtmlTargetElement("a", Attributes = "claim")]
    [HtmlTargetElement("button", Attributes = "claim")]
    public class AuthorizedButtonTagHelper : TagHelper
    {
        [HtmlAttributeName("claim")]
        public string Claim { get; set; } = string.Empty;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var user = ViewContext.HttpContext.User;

            if (user == null || !user.HasClaim("Permission", Claim))
            {
                // User does not have permission, do not render this element
                output.SuppressOutput();
                return;
            }

            // Remove the 'claim' attribute so it doesn't appear in the final HTML
            output.Attributes.RemoveAll("claim");

            // If it was the custom <authorized-button>, change it to a normal <button> or <a>
            if (output.TagName == "authorized-button")
            {
                var href = output.Attributes.FirstOrDefault(a => a.Name == "href");
                if (href != null)
                {
                    output.TagName = "a";
                }
                else
                {
                    output.TagName = "button";
                }
            }
        }
    }
}
