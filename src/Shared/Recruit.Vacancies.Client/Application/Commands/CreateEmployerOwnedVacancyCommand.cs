using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands;

public class CreateEmployerOwnedVacancyCommand : ICommand, IRequest<Unit>
{
    public Guid VacancyId { get; set; }
    public SourceOrigin Origin { get; set; }
    public string Title { get;set; }
    public string EmployerAccountId { get; set; }
    public VacancyUser User { get; set; }
    public UserType UserType { get; set; }
    public TrainingProvider TrainingProvider { get; set; }
    public string ProgrammeId { get; set; }
}