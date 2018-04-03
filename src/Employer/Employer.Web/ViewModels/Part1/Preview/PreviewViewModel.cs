using Humanizer;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Preview
{
    public class PreviewViewModel
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
    }
}
