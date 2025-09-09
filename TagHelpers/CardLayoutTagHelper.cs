using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotnetTestingWebApp.TagHelpers
{
    [HtmlTargetElement("card-layout")]
    public class CardLayoutTagHelper : TagHelper
    {
        public string Title { get; set; } = string.Empty;
        public string? ButtonCreateLink { get; set; }
        public Boolean ButtonMultipleDelete { get; set; } = false;


        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "card card-primary");

            var childContent = (await output.GetChildContentAsync()).GetContent();


            output.Content.SetHtmlContent($@"
                <div class='card-header'>
                    <h3 class='card-title'>{Title}</h3>
                    <div class='card-tools'>
                        {(string.IsNullOrEmpty(ButtonCreateLink) ? "" : $"<a href='{ButtonCreateLink}' class='btn btn-sm bg-gradient-success pull-right'><i class='fas fa-plus'></i> Tambah</a>")}
                        
                        {(ButtonMultipleDelete == true ? $"<a href='javascript:;' class='btn btn-sm bg-gradient-danger' onclick='hapusmulti()'><i class='fas fa-trash'></i> Delete Choose Data</a>" : "")}
                    </div>
                </div>
                <div class='card-body'>
                    {childContent}
                </div>
            ");
        }
    }
}