using System;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Shared.Web.Models;

public class SortParams<T> where T : Enum
{
    [FromQuery] public ColumnSortOrder? SortOrder { get; init; }
    [FromQuery] public T? SortColumn { get; init; }
}