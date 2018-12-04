using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class ValidationSummaryViewModel
    {
        public IList<string> OrderedFieldNames { get; set; } = new List<string>();
        public ModelStateDictionary ModelState { get; set; }
    }
}
