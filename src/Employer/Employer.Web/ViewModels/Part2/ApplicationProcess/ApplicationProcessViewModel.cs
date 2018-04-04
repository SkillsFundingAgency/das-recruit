using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class ApplicationProcessViewModel
    {
        public string Title { get; internal set; }
        public string ApplicationInstructions { get; internal set; }
        public string ApplicationUrl { get; internal set; }
        public string EmployerContactName { get; internal set; }
        public string EmployerContactEmail { get; internal set; }
        public string EmployerContactPhone { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ApplicationProcessEditModel.ApplicationInstructions),
            nameof(ApplicationProcessEditModel.ApplicationUrl),
            nameof(ApplicationProcessEditModel.EmployerContactName),
            nameof(ApplicationProcessEditModel.EmployerContactEmail),
            nameof(ApplicationProcessEditModel.EmployerContactPhone)
        };
    }
}
