using System;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;

internal class WebsiteValidator<T, TProperty>(IExternalWebsiteHealthCheckService externalWebsiteHealthCheckService) : AsyncPropertyValidator<T, TProperty> 
{
    public override async Task<bool> IsValidAsync(ValidationContext<T> context, TProperty value, CancellationToken cancellationToken)
    {
        if (value is not string url || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        try
        {
            return await externalWebsiteHealthCheckService.IsHealthyAsync(uri, cancellationToken);
        }
        catch (InvalidSchemeException)
        {
            return false;
        }
    }

    public override string Name => nameof(WebsiteValidator<T, string>);
}