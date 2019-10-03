using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Application.Communications;
using Humanizer;

namespace Recruit.Vacancies.Client.Application.Communications.CompositeDataItemProviderPlugins
{
    public class ApplicationsSubmittedCompositeDataItemPlugin : ICompositeDataItemProvider
    {
        public string ProviderName => CommunicationConstants.RequestType.ApplicationSubmitted;

        public Task<IEnumerable<CommunicationDataItem>> GetConsolidatedMessageDataItemsAsync(CommunicationMessage aggregateMessage, IEnumerable<CommunicationMessage> messages)
        {
            var dataItems = new List<CommunicationDataItem>();
            var vacancyRefs = GetUniqueVacancyRefs(messages);
            var header = GetNoApplicationsSubmittedHeader(messages, vacancyRefs);
            var vacanciesAndApplicationsSubmittedSnippets = GetPerVacancyApplicationsSubmittedSnippet(messages, vacancyRefs);
            var apprenticeshipServiceUrlDataItem = messages.First().DataItems.First(di => di.Key == CommunicationConstants.DataItemKeys.ApprenticeshipService.ApprenticeshipServiceUrl);

            dataItems.Add(new CommunicationDataItem(CommunicationConstants.DataItemKeys.Application.ApplicationsSubmittedAggregateHeader, header));
            dataItems.Add(new CommunicationDataItem(CommunicationConstants.DataItemKeys.Application.ApplicationsSubmittedAggregateBodySnippets, vacanciesAndApplicationsSubmittedSnippets));
            dataItems.Add(apprenticeshipServiceUrlDataItem);

            return Task.FromResult(dataItems.AsEnumerable());
        }

        private string GetPerVacancyApplicationsSubmittedSnippet(IEnumerable<CommunicationMessage> messages, List<string> vacancyRefs)
        {
            var vacanciesAndNewApplicationCounts = GetUniqueVacanciesAndNewApplicationCounts(messages, vacancyRefs);

            var sb = new StringBuilder();

            foreach (var vr in vacancyRefs)
            {
                var msg = messages.First(m => m.DataItems.Exists(di => di.Key == CommunicationConstants.DataItemKeys.Vacancy.VacancyReference && di.Value == vr));
                var applicationCount = vacanciesAndNewApplicationCounts.First(va => va.Key == long.Parse(vr)).Value;
                var vacancyTitle = msg.DataItems.Single(di => di.Key == CommunicationConstants.DataItemKeys.Vacancy.VacancyTitle).Value;
                var employerName = msg.DataItems.Single(di => di.Key == CommunicationConstants.DataItemKeys.Vacancy.EmployerName).Value;

                sb.AppendLine($"VAC{vr}: {vacancyTitle}");
                sb.AppendLine(employerName);
                sb.AppendLine($"{applicationCount} new {"application".ToQuantity(applicationCount, ShowQuantityAs.None)}");
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetNoApplicationsSubmittedHeader(IEnumerable<CommunicationMessage> messages, List<string> vacancyRefs)
        {
            var totalNoOfVacancies = vacancyRefs.Count();
            var totalNoOfApplications = messages.Count();

            var header = $"There has been {totalNoOfApplications} new {"application".ToQuantity(totalNoOfApplications, ShowQuantityAs.None)} for {totalNoOfVacancies} {"vacancy".ToQuantity(totalNoOfVacancies, ShowQuantityAs.None)}";
            return header;
        }

        private List<string> GetUniqueVacancyRefs(IEnumerable<CommunicationMessage> messages)
        {
            return messages
                    .SelectMany(m => m.DataItems
                        .Where(di => di.Key == CommunicationConstants.DataItemKeys.Vacancy.VacancyReference)
                        .Select(di => di.Value)
                    )
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<KeyValuePair<long, int>> GetUniqueVacanciesAndNewApplicationCounts(IEnumerable<CommunicationMessage> messages, List<string> vacancyRefs)
        {
            var vacancyRefApplicationCounts = messages
                                                .SelectMany(m => m.DataItems
                                                    .Where(di => di.Key == CommunicationConstants.DataItemKeys.Vacancy.VacancyReference && vacancyRefs.Contains(di.Value))
                                                    .GroupBy(di => di.Value)
                                                )
                                                .GroupBy(di => di.Key)
                                                .ToDictionary(
                                                        vacancyApplicationGroup => long.Parse(vacancyApplicationGroup.Key),
                                                        vacancyApplicationGroup => vacancyApplicationGroup.Count()
                                                    );
            return vacancyRefApplicationCounts;
        }
    }
}