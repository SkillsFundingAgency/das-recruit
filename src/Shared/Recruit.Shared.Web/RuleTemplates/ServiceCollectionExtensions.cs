using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRuleTemplates(this IServiceCollection services)
        {
            services.AddScoped<IRuleTemplateRunner, RuleTemplateRunner>();

            //Templates
            services.AddScoped<IRuleTemplate<ProfanityData>, ProfanityRuleTemplate>();
        }
    }
}
