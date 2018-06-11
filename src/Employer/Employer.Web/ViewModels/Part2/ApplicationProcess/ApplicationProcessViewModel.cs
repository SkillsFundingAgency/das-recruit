using Esfa.Recruit.Employer.Web.Views;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationProcessViewModel
    {
        public string Title { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }

        public static string PreviewSectionAnchor => PreviewAnchors.ApplicationProcessSection;

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ApplicationProcessEditModel.ApplicationUrl),
            nameof(ApplicationProcessEditModel.ApplicationInstructions)
        };
    }
}
