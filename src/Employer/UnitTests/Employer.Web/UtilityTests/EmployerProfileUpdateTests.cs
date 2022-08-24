using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Shared.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.UtilityTests
{
    public class EmployerProfileUpdateTests
    {
        [Test, MoqAutoData]
        public async Task Then_Does_Not_Update_If_No_Values_Supplied(
            EmployerProfile employerProfile,
            VacancyUser vacancyUser,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            Utility utility)
        {
            await utility.UpdateEmployerProfile(null, employerProfile, null, vacancyUser);
            
            recruitVacancyClient.Verify(x=>x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), vacancyUser), Times.Never);
        }
        
        [Test, MoqAutoData]
        public async Task Then_Updates_Profile_If_Has_Address(
            Address address,
            VacancyUser vacancyUser,
            EmployerProfile employerProfile,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            Utility utility)
        {
            await utility.UpdateEmployerProfile(null, employerProfile, address, vacancyUser);
            
            recruitVacancyClient.Verify(x=>x.UpdateEmployerProfileAsync(It.Is<EmployerProfile>(c=>c.OtherLocations.Contains(address)), vacancyUser));
        }
        
        [Test, MoqAutoData]
        public async Task Then_Updates_Profile_If_Has_New_Trading_Name(
            string tradingName,
            Address address,
            VacancyUser vacancyUser,
            EmployerProfile employerProfile,
            VacancyEmployerInfoModel model,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            Utility utility)
        {
            
            model.NewTradingName = tradingName;
            model.EmployerIdentityOption = EmployerIdentityOption.NewTradingName;
            
            await utility.UpdateEmployerProfile(model, employerProfile, address, vacancyUser);
            
            recruitVacancyClient.Verify(x=>x.UpdateEmployerProfileAsync(It.Is<EmployerProfile>(c=>c.TradingName.Equals(tradingName)), vacancyUser));
        }

        [Test, MoqAutoData]
        public async Task Then_Does_Not_Update_Trading_Name_If_Not_New(
            Address address,
            VacancyUser vacancyUser,
            EmployerProfile employerProfile,
            VacancyEmployerInfoModel model,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            Utility utility)
        {
            model.EmployerIdentityOption = EmployerIdentityOption.ExistingTradingName;
            
            await utility.UpdateEmployerProfile(null, employerProfile, null, vacancyUser);
            
            recruitVacancyClient.Verify(x=>x.UpdateEmployerProfileAsync(It.IsAny<EmployerProfile>(), vacancyUser), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_If_No_AccountLegalEntityPublicHashedId_With_Profile_Then_Updated(
            Address address,
            VacancyUser vacancyUser,
            EmployerProfile employerProfile,
            VacancyEmployerInfoModel model,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            Utility utility)
        {
            employerProfile.AccountLegalEntityPublicHashedId = "";
            
            await utility.UpdateEmployerProfile(model, employerProfile, address, vacancyUser);
            
            recruitVacancyClient.Verify(x=>x.UpdateEmployerProfileAsync(It.Is<EmployerProfile>(c=>c.AccountLegalEntityPublicHashedId.Equals(model.AccountLegalEntityPublicHashedId)), vacancyUser));
        }
    }
}