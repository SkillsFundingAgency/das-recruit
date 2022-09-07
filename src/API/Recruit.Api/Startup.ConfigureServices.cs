using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Ioc;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Recruit.Api.Configuration;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Services;

namespace SFA.DAS.Recruit.Api
{
    public partial class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            services.Configure<RecruitConfiguration>(Configuration.GetSection("Recruit"));
            services.Configure<AzureActiveDirectoryConfiguration>(Configuration.GetSection("AzureAd"));

            services.AddScoped(provider => {
                var httpContext = provider.GetRequiredService<IHttpContextAccessor>().HttpContext;

                if (httpContext.Request.RouteValues["Controller"].ToString()!.Equals("Vacancies", StringComparison.CurrentCultureIgnoreCase)
                   && (httpContext.Request.RouteValues["Action"].ToString()!.Equals("CreateTraineeship", StringComparison.CurrentCultureIgnoreCase)
                   || httpContext.Request.RouteValues["Action"].ToString()!.Equals("ValidateTraineeship", StringComparison.CurrentCultureIgnoreCase)))
                {
                    return new ServiceParameters(VacancyType.Traineeship.ToString());
                }
                return new ServiceParameters(VacancyType.Apprenticeship.ToString());
            });


            var azureAdConfig = Configuration
                .GetSection("AzureAd")
                .Get<AzureActiveDirectoryConfiguration>();
            
            var policies = new Dictionary<string, string>
            {
                {PolicyNames.Default, "Default"},
            };
            services.AddAuthentication(azureAdConfig, policies);

            services.AddMediatR(typeof(Startup).Assembly, typeof(CreateApplicationReviewCommand).Assembly);

            services.AddSingleton<IVacancySummaryMapper, VacancySummaryMapper>();
            services.AddSingleton<IQueryStoreReader, QueryStoreClient>();

            services.AddRecruitStorageClient(Configuration);
            
            MongoDbConventions.RegisterMongoConventions();

            services.AddHealthChecks()
                    .AddMongoDb(Configuration.GetConnectionString("MongoDb"))
                    .AddApplicationInsightsPublisher();

            services.AddApplicationInsightsTelemetry();

            services
                .AddMvc(o =>
                {
                    if (HostingEnvironment.IsDevelopment() == false)
                    {
                        o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>()));
                    }
                    o.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RecruitAPI", Version = "v1" });
                
            });
        }
    }
}