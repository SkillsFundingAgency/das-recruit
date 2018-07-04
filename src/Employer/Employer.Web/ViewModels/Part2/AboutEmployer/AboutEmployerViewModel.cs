using Esfa.Recruit.Employer.Web.Views;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class AboutEmployerViewModel
    {
        public string Title { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }

        public static string PreviewSectionAnchor => PreviewAnchors.EmployerSection;

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(AboutEmployerEditModel.EmployerDescription),
            nameof(AboutEmployerEditModel.EmployerWebsiteUrl)
        };
    }
}
