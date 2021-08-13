using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Preview
{
    public class SubmitReviewModel : VacancyRouteModel, IValidatableObject
    {
        private const int RejectedReasonMaxCharacters = 200;

        [Required(ErrorMessage = "You need to submit or reject the advert")]
        public bool? SubmitToEsfa { get; set; }

        public string RejectedReason { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var property = new[] { nameof(RejectedReason) };

            if(SubmitToEsfa.HasValue && SubmitToEsfa.Value == false)
            {
                if (string.IsNullOrEmpty(RejectedReason))
                {
                    yield return new ValidationResult("Enter details of why you are rejecting this advert", property);
                }
                else if (RejectedReason.Length > RejectedReasonMaxCharacters)
                {
                    yield return new ValidationResult($"You have {RejectedReason.Length- RejectedReasonMaxCharacters} character too many", property);
                }
            }
        }
    }
}
