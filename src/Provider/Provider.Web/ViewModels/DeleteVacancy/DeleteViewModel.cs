namespace Esfa.Recruit.Provider.Web.ViewModels.DeleteVacancy
{
    public class DeleteViewModel : DeleteEditModel
    {
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
    }
}