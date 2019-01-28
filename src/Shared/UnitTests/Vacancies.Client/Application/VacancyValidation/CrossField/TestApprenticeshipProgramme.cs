using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.CrossField
{
    public class TestApprenticeshipProgramme : IApprenticeshipProgramme
    {
        public string Id { get; set; }

        public TrainingType ApprenticeshipType  { get; set; }

        public string Title { get; set; }

        public DateTime? EffectiveFrom  { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public ProgrammeLevel Level  { get; set; }

        public int Duration { get; set; }
    }
}