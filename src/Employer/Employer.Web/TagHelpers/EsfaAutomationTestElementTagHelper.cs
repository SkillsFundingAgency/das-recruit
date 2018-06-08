using Esfa.Recruit.Shared.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.TagHelpers
{
    [HtmlTargetElement(Attributes = TagAttributeName)]
    public class EsfaAutomationTestElementTagHelper : TagHelper
    {
        private const string TagAttributeName = "esfa-automation";
        private const string DataAutomationAttributeName = "data-automation";
        private readonly IHostingEnvironment _env;

        public EsfaAutomationTestElementTagHelper(IHostingEnvironment env)
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