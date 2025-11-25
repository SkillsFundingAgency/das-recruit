using System.Threading;
using Esfa.Recruit.Provider.Web.Validation;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Validation;

public class WhenValidatingVacancyLegalEntityExists
{
    [Test, MoqAutoData]
    public async Task Then_The_Legal_Entity_Is_Valid(
        Vacancy vacancy,
        LegalEntity legalEntity,
        [Frozen] Mock<ILegalEntityAgreementService> legalEntityAgreementService,
        [Greedy] LegalEntityExistsValidator sut)
    {
        // arrange
        legalEntityAgreementService
            .Setup(x => x.GetLegalEntityAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId))
            .ReturnsAsync(legalEntity);

        // act
        var result = await sut.ValidateAsync(vacancy, CancellationToken.None);

        // assert
        result.IsValid.Should().BeTrue();
    }
    
    [Test, MoqAutoData]
    public async Task Then_The_Legal_Entity_Is_Invalid(
        Vacancy vacancy,
        LegalEntity legalEntity,
        [Frozen] Mock<ILegalEntityAgreementService> legalEntityAgreementService,
        [Greedy] LegalEntityExistsValidator sut)
    {
        // arrange
        legalEntityAgreementService
            .Setup(x => x.GetLegalEntityAsync(vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId))
            .ReturnsAsync((LegalEntity)null);

        // act
        var result = await sut.ValidateAsync(vacancy, CancellationToken.None);

        // assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AccountLegalEntityPublicHashedId));
        result.Errors[0].ErrorMessage.Should().Be("Enter a valid employer name");
    }
}