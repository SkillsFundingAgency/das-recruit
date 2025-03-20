using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Recruit.Api.Commands;
using SFA.DAS.Recruit.Api.Extensions;
using SFA.DAS.Recruit.Api.Mappers;
using SFA.DAS.Recruit.Api.Models;
using SFA.DAS.Recruit.Api.Queries;

namespace SFA.DAS.Recruit.Api.Controllers;

[Route("api/[controller]")]
public class VacanciesController(IMediator mediator) : ApiControllerBase
{
    private static readonly Dictionary<string, string> SimpleFieldMappings = new()
    {
        { "EmployerLocation", "Address" },
        { "EmployerLocations", "Addresses" },
    };
    
    private static readonly Dictionary<string, string> ComplexFieldMappings = new()
    {
        { @"EmployerLocations\[(?<1>\d)\].Country", "Addresses[{0}].Country" }
    };

    private static void MapValidationErrorsToModel(CreateVacancyCommandResponse response)
    {
        if (response.ValidationErrors is not { Count: > 0 })
        {
            return;
        }
            
        // We need to map internal vacancy properties to the model we accept.
        foreach (var validationError in response.ValidationErrors.OfType<DetailedValidationError>())
        {
            if (!PerformSimpleMapping(validationError))
            {
                PerformComplexMapping(validationError);
            }
        }
    }

    private static bool PerformSimpleMapping(DetailedValidationError validationError)
    {
        bool result = SimpleFieldMappings.TryGetValue(validationError.Field, out string fieldMapping);
        if (result)
        {
            validationError.Field = fieldMapping;
        }

        return result;
    }

    private static void PerformComplexMapping(DetailedValidationError validationError)
    {
        foreach (var fieldMapping in ComplexFieldMappings)
        {
            validationError.Field = validationError.Field.RegexReplaceWithGroups(fieldMapping.Key, fieldMapping.Value);
        }
    }

    // GET api/vacancies
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string employerAccountId, ulong? ukprn, uint pageSize = 25, uint pageNo = 1)
    {
        var resp = await mediator.Send(new GetVacanciesQuery(employerAccountId.Trim().ToUpper(), (long?)ukprn, (int)pageSize, (int)pageNo));
        return GetApiResponse(resp);
    }

    [HttpPost]
    [Route("{id}")]
    public async Task<IActionResult> Create([FromRoute] Guid id, CreateVacancyRequest request, [FromQuery] string userEmail = null, [FromQuery] long? ukprn = null)
    {
        var resp = await mediator.Send(new CreateVacancyCommand
        {
            Vacancy = request.MapFromCreateVacancyRequest(id),
            VacancyUserDetails = new VacancyUser
            {
                Email = userEmail,
                Ukprn = ukprn
            },
        });

        MapValidationErrorsToModel(resp);

        return GetApiResponse(resp);
    }

    [HttpPost]
    [Route("{id}/validate")]
    public async Task<IActionResult> Validate([FromRoute] Guid id, CreateVacancyRequest request, [FromQuery] string userEmail = null, [FromQuery] long? ukprn = null)
    {
        var resp = await mediator.Send(new CreateVacancyCommand
        {
            Vacancy = request.MapFromCreateVacancyRequest(id),
            VacancyUserDetails = new VacancyUser
            {
                Email = userEmail,
                Ukprn = ukprn
            },
            ValidateOnly = true
        });
            
        MapValidationErrorsToModel(resp);

        return GetApiResponse(resp);
    }
}