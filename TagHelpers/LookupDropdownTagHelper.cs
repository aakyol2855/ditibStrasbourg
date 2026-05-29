using DitibStasbourg.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DitibStasbourg.TagHelpers
{
    [HtmlTargetElement("lookup-dropdown", Attributes = "type")]
    public class LookupDropdownTagHelper : TagHelper
    {
        private readonly ILookupService _lookupService;

        public LookupDropdownTagHelper(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HtmlAttributeName("type")]
        public string TypeCode { get; set; } = string.Empty;

        [HtmlAttributeName("asp-for")]
        public Microsoft.AspNetCore.Mvc.ViewFeatures.ModelExpression? For { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "select";
            output.Attributes.SetAttribute("class", "form-select"); // Bootstrap 5 default select class
            
            if (For != null)
            {
                output.Attributes.SetAttribute("name", For.Name);
                output.Attributes.SetAttribute("id", For.Name.Replace(".", "_"));
            }

            var values = await _lookupService.GetDynamicValuesAsync(TypeCode);

            output.Content.AppendHtml("<option value=\"\">Seçiniz</option>");

            foreach (var val in values)
            {
                var selected = (For?.Model != null && For.Model.ToString() == val.Id.ToString()) ? "selected" : "";
                output.Content.AppendHtml($"<option value=\"{val.Id}\" {selected}>{val.Name}</option>");
            }
        }
    }
}
