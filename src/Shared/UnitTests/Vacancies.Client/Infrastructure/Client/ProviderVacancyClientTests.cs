using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client
{
    public class ProviderVacancyClientTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Parameters_Are_Passed_To_The_Command_And_Handled(
            long ukprn, 
            DateTime fromDate, 
            DateTime toDate, 
            VacancyUser user, 
            string reportName, 
            VacancyType vacancyType,
            [Frozen] Mock<IMessaging> messaging,
            VacancyClient client)
        {
            await client.CreateProviderApplicationsReportAsync(ukprn, fromDate, toDate, user, reportName, vacancyType);
            
            messaging.Verify(x=>x.SendCommandAsync(It.Is<CreateReportCommand>(c=>
                c.ReportId != Guid.Empty &&
                c.Owner.Ukprn.Equals(ukprn) &&
                c.Owner.OwnerType.Equals(ReportOwnerType.Provider) &&
                c.Parameters["VacancyType"].Equals(vacancyType.ToString()) &&
                c.Parameters["Ukprn"].Equals(ukprn)
                )));
        }
    }
}