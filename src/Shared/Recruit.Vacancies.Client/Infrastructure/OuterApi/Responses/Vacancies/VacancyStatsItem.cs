namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;

public record struct VacancyStatsItem(
    int Applications = 0,
    int NewApplications = 0,
    int SharedApplications = 0,
    int AllSharedApplications = 0,
    int SuccessfulApplications = 0,
    int UnsuccessfulApplications = 0,
    int EmployerReviewedApplications = 0,
    int InterviewingApplications = 0,
    int InReviewApplications = 0,
    int EmployerInterviewingApplications = 0,
    bool HasNoApplications = false);