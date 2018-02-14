using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web
{
    public partial class Startup
    {
        private void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc(opts =>
            {
                //opts.Conventions.Insert(0, new EmployerAccountRoutePrefixConvention());

                if (!_hostingEnvironment.IsDevelopment())
                {
                    opts.Filters.Add(new RequireHttpsAttribute());
                }

                if (!_isAuthEnabled)
                {
                    opts.Filters.Add(new AllowAnonymousFilter());
                }

                var policy = new AuthorizationPolicyBuilder()
                         .RequireAuthenticatedUser()
                         .Build();

                opts.Filters.Add(new AuthorizeFilter(policy));

                opts.Filters.Add(new AuthorizeFilter("HasEmployerAccount"));

                var jsonInputFormatters = opts.InputFormatters.OfType<JsonInputFormatter>();
                foreach (var formatter in jsonInputFormatters)
                {
                    formatter.SupportedMediaTypes
                        .Add(MediaTypeHeaderValue.Parse("application/csp-report"));
                }

                opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
        }
    }
}
