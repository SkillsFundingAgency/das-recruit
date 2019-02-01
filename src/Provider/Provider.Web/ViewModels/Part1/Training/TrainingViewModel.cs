using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Validations;
using ErrMsg = Esfa.Recruit.Shared.Web.ViewModels.ValidationMessages.DateValidationMessages;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : VacancyRouteModel
    {
        public string ClosingDay { get; set; }
        public string ClosingMonth { get; set; }
        public string ClosingYear { get; set; }

        [TypeOfDate(ErrorMessage = ErrMsg.TypeOfDate.ClosingDate)]
        public string ClosingDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ClosingDay) || 
                    string.IsNullOrWhiteSpace(ClosingMonth) || 
                    string.IsNullOrWhiteSpace(ClosingYear))
                {
                    return null;
                }
                return $"{ClosingDay}/{ClosingMonth}/{ClosingYear}";
            }
        }
        public string StartDay { get; set; }
        public string StartMonth { get; set; }
        public string StartYear { get; set; }

        [TypeOfDate(ErrorMessage = ErrMsg.TypeOfDate.StartDate)]
        public string StartDate
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StartDay) ||
                    string.IsNullOrWhiteSpace(StartMonth) ||
                    string.IsNullOrWhiteSpace(StartYear))
                {
                    return null;
                }
                return $"{StartDay}/{StartMonth}/{StartYear}";
            }
        }
        public string SelectedProgrammeId { get; set; }

        public bool IsDisabilityConfident { get; set; }
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }

        //public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ClosingDate),
            nameof(StartDate),
            nameof(SelectedProgrammeId)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }

        public int CurrentYear { get; set; }
    }
}