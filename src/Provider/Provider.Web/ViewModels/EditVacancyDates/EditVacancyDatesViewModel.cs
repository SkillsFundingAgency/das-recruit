using System.Collections.Generic;

namespace Esfa.Recruit.Provider.Web.ViewModels.EditVacancyDates
{
    public class EditVacancyDatesViewModel
    {
        public string ClosingDay { get; set; }
        public string ClosingMonth { get; set; }
        public string ClosingYear { get; set; }

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

        public string ProgrammeName { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(EditVacancyDatesEditModel.ClosingDate),
            nameof(EditVacancyDatesEditModel.StartDate)
        };

        public int CurrentYear { get; set; }
    }
}