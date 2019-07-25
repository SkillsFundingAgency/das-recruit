using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.ViewModels.WithdrawVacancy
{
    public class AcknowledgeEditModel
    {
        [FromRoute]
        public string VacancyReference { get; set; }

        public bool Acknowledged { get; set; }
    }
}
