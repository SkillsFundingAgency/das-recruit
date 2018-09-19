using System;
using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Qa.Web.ViewModels.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class UnassignReviewEditModel
    {
        [FromRoute]
        public Guid ReviewId { get; set; }

        [Required(ErrorMessage = ValidationMessages.UnassignReviewConfirmationMessages.SelectionRequired)]
        public bool? ConfirmUnassign { get; set; }
    }
}
