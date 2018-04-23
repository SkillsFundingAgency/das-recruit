﻿using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Validations;
using ErrMsg = Esfa.Recruit.Employer.Web.ViewModels.ValidationMessages.TrainingValidationMessages;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingEditModel : VacancyRouteModel
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
    }
}
