using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Ioc;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
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

            var azureAdConfig = Configuration
                .GetSection("AzureAd")
                .Get<AzureActiveDirectoryConfiguration>();
            
            SetupAuthorization(services, azureAdConfig);

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
                        o.Filters.Add(new AuthorizeFilter("default"));
                    }
                    o.Conventions.Add(new ApiExplorerGroupPerVersionConvention());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RecruitAPI", Version = "v1" });
                
            });
        }

        private void SetupAuthorization(IServiceCollection services, AzureActiveDirectoryConfiguration azureActiveDirectoryConfiguration)
        {
            if (HostingEnvironment.IsDevelopment() == false)
            {
                services.AddAuthorization(o =>
                {
                    o.AddPolicy("default", policy => { policy.RequireAuthenticatedUser(); });
                });

                services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                    .AddJwtBearer(auth =>
                    {
                        auth.Authority =
                            $"https://login.microsoftonline.com/{azureActiveDirectoryConfiguration.Tenant}";
                        auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidAudiences = new List<string>
                            {
                                azureActiveDirectoryConfiguration.Identifier,
                                azureActiveDirectoryConfiguration.Id
                            }
                        };
                    });

                services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
            }
        }
    }
}