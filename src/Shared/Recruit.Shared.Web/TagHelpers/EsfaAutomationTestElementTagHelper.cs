using System.Threading.Tasks;
using Esfa.Recruit.Shared.Web.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace Esfa.Recruit.Shared.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = TagAttributeName)]
    public class EsfaAutomationTestElementTagHelper : TagHelper
    {
        private const string TagAttributeName = "esfa-automation";
        private const string DataAutomationAttributeName = "data-automation";
        private readonly IWebHostEnvironment _env;

        public EsfaAutomationTestElementTagHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HtmlAttributeName(TagAttributeName)]
        public string TargetName { get; set; }
        
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!_env.IsEnvironment(EnvironmentNames.PROD))
            {
                output.Attributes.SetAttribute(DataAutomationAttributeName, TargetName);
            }

            return Task.CompletedTask;
        }
    }
}