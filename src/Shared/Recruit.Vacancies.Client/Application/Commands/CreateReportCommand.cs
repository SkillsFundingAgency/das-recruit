using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateReportCommand : ICommand, IRequest<Unit>
    {
        public CreateReportCommand(Guid reportId, ReportOwner owner, ReportType reportType, Dictionary<string, object> parameters, VacancyUser requestedBy, string reportName)
        {
            ReportId = reportId;
            Owner = owner;
            ReportType = reportType;
            Parameters = parameters;
            RequestedBy = requestedBy;
            ReportName = reportName;
        }

        public Guid ReportId { get; set; }
        public ReportOwner Owner { get; set; }
        public ReportType ReportType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public VacancyUser RequestedBy { get; set; }
        public string ReportName { get; set; }
    }
}
