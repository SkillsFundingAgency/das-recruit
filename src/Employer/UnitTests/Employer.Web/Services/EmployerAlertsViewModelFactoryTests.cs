using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Services;

public class EmployerAlertsViewModelFactoryTests
{
    [Test, MoqAutoData]
    public async Task When_Calling_Create_Then_Builds_ViewModel_Correctly(
        string employerAccountId, 
        User user,
        [Frozen] Mock<IEmployerVacancyClient> mockEmployerVacancyClient,
        EmployerAlertsViewModelFactory factory)
    {
        var viewModel = await factory.Create(employerAccountId, user);

        viewModel.Should().NotBeNull();
        viewModel.BlockedProviderAlert.Should().BeNull();
        viewModel.BlockedProviderTransferredVacanciesAlert.Should().BeNull();
        viewModel.EmployerRevokedTransferredVacanciesAlert.Should().BeNull();
        viewModel.WithdrawnByQaVacanciesAlert.Should().BeNull();
    }
}