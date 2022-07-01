using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Commands;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers
{
    [Route("api/[controller]")]
    public class VacanciesController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        public VacanciesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/vacancies
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string employerAccountId, uint? legalEntityId, ulong? ukprn, uint pageSize = 25, uint pageNo = 1)
        {
            var resp = await _mediator.Send(new GetVacanciesQuery(employerAccountId.Trim().ToUpper(), (int?)legalEntityId, (long?)ukprn, (int)pageSize, (int)pageNo));
            return GetApiResponse(resp);
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> Create([FromRoute] Guid id, CreateVacancyRequest request, [FromQuery] string userEmail = null, [FromQuery] long? ukprn = null)
        {
            var resp = await _mediator.Send(new CreateVacancyCommand
            {
                Vacancy = request.MapFromCreateVacancyRequest(id),
                VacancyUserDetails = new VacancyUser
                {
                    Email = userEmail,
                    Ukprn = ukprn
                }
            });

            return GetApiResponse(resp);
        }

        [HttpPost]
        [Route("{id}/validate")]
        public async Task<IActionResult> Validate([FromRoute] Guid id, CreateVacancyRequest request, [FromQuery] string userEmail = null, [FromQuery] long? ukprn = null)
        {
            var resp = await _mediator.Send(new CreateVacancyCommand
            {
                Vacancy = request.MapFromCreateVacancyRequest(id),
                VacancyUserDetails = new VacancyUser
                {
                    Email = userEmail,
                    Ukprn = ukprn
                },
                ValidateOnly = true
            });

            return GetApiResponse(resp);
        }

        [HttpPost]
        [Route("createtraineeship/{id}")]
        public async Task<IActionResult> CreateTraineeship([FromRoute] Guid id, CreateTraineeshipVacancyRequest request, [FromQuery] string userEmail = null, [FromQuery] long? ukprn = null)
        {
            var resp = await _mediator.Send(new CreateTraineeshipVacancyCommand
            {
                Vacancy = request.MapFromCreateTraineeshipVacancyRequest(id),
                VacancyUserDetails = new VacancyUser
                {
                    Email = userEmail,
                    Ukprn = ukprn
                }
            });

            return GetApiResponse(resp);
        }

        [HttpPost]
        [Route("{id}/ValidateTraineeship")]
        public async Task<IActionResult> ValidateTraineeship([FromRoute] Guid id, CreateTraineeshipVacancyRequest request, [FromQuery] string userEmail = null, [FromQuery] long? ukprn = null)
        {
            var resp = await _mediator.Send(new CreateTraineeshipVacancyCommand
            {
                Vacancy = request.MapFromCreateTraineeshipVacancyRequest(id),
                VacancyUserDetails = new VacancyUser
                {
                    Email = userEmail,
                    Ukprn = ukprn
                },
                ValidateOnly = true
            });

            return GetApiResponse(resp);
        }
    }
}
