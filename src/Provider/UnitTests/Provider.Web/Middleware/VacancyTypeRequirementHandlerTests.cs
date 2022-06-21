﻿using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Middleware;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Middleware
{
    public class VacancyTypeRequirementHandlerTests
    {
        [Fact]
        public async Task WhenCallingHandle_AndVacancyTypeMatches_ThenHasSucceededIsTrue()
        {
            var requirement = new VacancyTypeRequirement(VacancyType.Traineeship);
            var context = new AuthorizationHandlerContext(new[] {requirement}, new ClaimsPrincipal(Mock.Of<IIdentity>()), null);
            var serviceParameters = new ServiceParameters(VacancyType.Traineeship.ToString());
            var handler = new VacancyTypeRequirementHandler(serviceParameters);

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();
        }
        
        [Fact]
        public async Task WhenCallingHandle_AndVacancyTypeNotMatching_ThenHasSucceededIsFalse()
        {
            var requirement = new VacancyTypeRequirement(VacancyType.Traineeship);
            var context = new AuthorizationHandlerContext(new[] {requirement}, new ClaimsPrincipal(Mock.Of<IIdentity>()), null);
            var serviceParameters = new ServiceParameters(VacancyType.Apprenticeship.ToString());
            var handler = new VacancyTypeRequirementHandler(serviceParameters);

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
        }
    }
}