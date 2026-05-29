using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DitibStasbourg.TagHelpers
{
    [HtmlTargetElement("help-icon")]
    public class HelpIconTagHelper : TagHelper
    {
        public int TopicId { get; set; }
        public string? Title { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", "#");
            output.Attributes.SetAttribute("class", "text-primary ms-2 open-modal");
            output.Attributes.SetAttribute("data-url", $"/Help/Details/{TopicId}");
            output.Attributes.SetAttribute("data-modal-title", Title ?? "Yardım");
            output.Attributes.SetAttribute("title", "Yardım Al");
            
            output.Content.SetHtmlContent("<i class='bi bi-question-circle'></i>");
        }
    }
}
