using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotnetTestingWebApp.TagHelpers
{
    [HtmlTargetElement("card-layout")]
    public class CardLayoutTagHelper(IHtmlHelper htmlHelper) : TagHelper
    {
        public string Title { get; set; } = string.Empty;
        public string? ButtonCreateLink { get; set; }
        public Boolean ButtonMultipleDelete { get; set; } = false;
        private readonly IHtmlHelper _htmlHelper = htmlHelper;

        [ViewContext] // supaya HtmlHelper tahu konteks View
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "card card-primary");

            var childContent = (await output.GetChildContentAsync()).GetContent();

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                { "LinkPath", "/Product/Create" }
            };

            var buttonCreate = await _htmlHelper.PartialAsync(
                "Components/_ButtonCreate", viewData
            );

            var buttonMultiDelete = await _htmlHelper.PartialAsync("Components/_ButtonMultiDelete");

            output.Content.AppendHtml("<div class='card-header'>");
            output.Content.AppendHtml($"<h3 class='card-title'>{Title}</h3>");
            output.Content.AppendHtml("<div class='card-tools'>");

            if (!string.IsNullOrEmpty(ButtonCreateLink))
            {
                output.Content.AppendHtml(buttonCreate);
            }
            // tombol delete multi
            if (ButtonMultipleDelete == true)
            {
                output.Content.AppendHtml(" ");
                output.Content.AppendHtml(buttonMultiDelete);
            }

            output.Content.AppendHtml("</div></div>"); // close card-tools + card-header
            // card body
            output.Content.AppendHtml("<div class='card-body'>");
            output.Content.AppendHtml(childContent);
            output.Content.AppendHtml("</div>");

            /* output.Content.SetHtmlContent($@"
                <div class='card-header'>
                    <h3 class='card-title'>{Title}</h3>
                    <div class='card-tools'>
                        {(string.IsNullOrEmpty(ButtonCreateLink) ? "" : $"{buttonCreate}")}
                        
                        {(ButtonMultipleDelete == true ? $"<a href='javascript:;' class='btn btn-sm bg-gradient-danger' onclick='hapusmulti()'><i class='fas fa-trash'></i> Delete Choose Data</a>" : "")}
                    </div>
                </div>
                <div class='card-body'>
                    {childContent}
                </div>
            "); */
        }
    }
}