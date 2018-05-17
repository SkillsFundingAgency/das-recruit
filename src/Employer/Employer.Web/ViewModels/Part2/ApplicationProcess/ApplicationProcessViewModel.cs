using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationProcessViewModel
    {
        public string Title { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ApplicationProcessEditModel.ApplicationUrl),
            nameof(ApplicationProcessEditModel.ApplicationInstructions)
        };
    }
}
