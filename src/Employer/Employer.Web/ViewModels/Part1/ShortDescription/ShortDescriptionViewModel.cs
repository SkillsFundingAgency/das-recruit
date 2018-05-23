using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.ShortDescription
{
    public class ShortDescriptionViewModel : ShortDescriptionEditModel
    {
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ShortDescription)
        };
    }
}
