using Humanizer;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.SearchResultPreview
{
    public class SearchResultPreviewViewModel
    {
        public string EmployerName { get; set; }

        public string NumberOfPositions { get; set; }

        public string NumberOfPositionsCaption =>   !string.IsNullOrEmpty(NumberOfPositions) && int.TryParse(NumberOfPositions, out var positions) 
                                                    ? $"{"position".ToQuantity(positions)} available" 
                                                    : string.Empty;

        public string ShortDescription { get; set; }

        public string ClosingDate { get; set; }

        public string StartDate { get; set; }

        public string LevelName { get; set; }

        public string Wage { get; set; }

        public string MapUrl { get; set; }

        public string Title { get; internal set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions),
            nameof(ShortDescription),
            nameof(ClosingDate),
            nameof(StartDate),
            nameof(LevelName),
            nameof(Wage)
        };
    }
}