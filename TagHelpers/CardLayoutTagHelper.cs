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
        public string? ButtonRecycleLink { get; set; }
        public string? ButtonExportExcel { get; set; }
        public Boolean ButtonMultipleDelete { get; set; } = false;
        public Boolean ButtonImportExcel { get; set; } = false;
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

            var buttonMultiDelete = await _htmlHelper.PartialAsync("Components/_ButtonMultiDelete");


            output.Content.AppendHtml("<div class='card-header'>");
            output.Content.AppendHtml($"<h3 class='card-title'>{Title}</h3>");
            output.Content.AppendHtml("<div class='card-tools'>");

            //component button recycle
            if (ButtonImportExcel == true)
            {
                var buttonImportExcel = await _htmlHelper.PartialAsync("Components/_ButtonImportExcel");
                output.Content.AppendHtml(buttonImportExcel);
                output.Content.AppendHtml(" ");
            }

            if (!string.IsNullOrEmpty(ButtonExportExcel))
            {
                var parts = ButtonExportExcel.Split('/');
                string controller = parts.Length > 0 ? parts[0] : "";
                string action = parts.Length > 1 ? parts[1] : "";
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    { "LinkController", controller },
                    { "LinkAction", action }
                };

                var buttonRecycle = await _htmlHelper.PartialAsync(
                    "Components/_ButtonExport", viewData
                );
                output.Content.AppendHtml(buttonRecycle);
            }

            if (!string.IsNullOrEmpty(ButtonCreateLink))
            {
                var parts = ButtonCreateLink.Split('/');
                string controller = parts.Length > 0 ? parts[0] : "";
                string action = parts.Length > 1 ? parts[1] : "";
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    { "LinkController", controller },
                    { "LinkAction", action }
                };

                var buttonCreate = await _htmlHelper.PartialAsync(
                    "Components/_ButtonCreate", viewData
                );
                output.Content.AppendHtml(buttonCreate);
            }
            //component button recycle
            if (!string.IsNullOrEmpty(ButtonRecycleLink))
            {
                var parts = ButtonRecycleLink.Split('/');
                string controller = parts.Length > 0 ? parts[0] : "";
                string action = parts.Length > 1 ? parts[1] : "";
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    { "LinkController", controller },
                    { "LinkAction", action }
                };

                var buttonRecycle = await _htmlHelper.PartialAsync(
                    "Components/_ButtonRecycle", viewData
                );
                output.Content.AppendHtml(buttonRecycle);
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