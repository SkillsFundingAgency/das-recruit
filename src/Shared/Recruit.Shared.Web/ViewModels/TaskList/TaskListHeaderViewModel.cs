namespace Esfa.Recruit.Shared.Web.ViewModels.TaskList;

public record struct TaskListHeaderViewModel(int SectionNumber, string SectionTitle, VacancyTaskListSectionState SectionState);