using System;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Qa.Web.ViewModels.Validations;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class UnassignReviewViewModel
    {
        public string AdvisorName { get; set; }

        [Required(ErrorMessage = ValidationMessages.UnassignReviewConfirmationMessages.SelectionRequired)]
        public bool? ConfirmUnassign { get; set; }

        public Guid ReviewId { get; set; }
        public string Title { get; set; }
    }
}
