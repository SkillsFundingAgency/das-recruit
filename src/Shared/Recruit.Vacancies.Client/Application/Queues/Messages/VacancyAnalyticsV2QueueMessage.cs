namespace Esfa.Recruit.Vacancies.Client.Application.Queues.Messages;

public record VacancyAnalyticsV2QueueMessage
{
    public string VacancyReference { get; set; }
    public int ViewsCount { get; set; }
    public int SearchResultsCount { get; set; }
    public int ApplicationStartedCount { get; set; }
    public int ApplicationSubmittedCount { get; set; }
}