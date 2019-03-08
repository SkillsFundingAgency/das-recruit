using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using MediatR;

namespace Esfa.Recruit.Vacancies.Client.Application.Commands
{
    public class CreateReportCommand : ICommand, IRequest
    {
        public CreateReportCommand(Guid reportId, ReportType reportType, List<ReportParameter> parameters, VacancyUser requestedBy)
        {
            ReportId = reportId;
            ReportType = reportType;
            Parameters = parameters;
            RequestedBy = requestedBy;
        }

        public Guid ReportId { get; set; }
        public ReportType ReportType { get; set; }
        public List<ReportParameter> Parameters { get; set; }
        public VacancyUser RequestedBy { get; set; }
    }
}
