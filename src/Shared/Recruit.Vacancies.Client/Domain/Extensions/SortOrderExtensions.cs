using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions;

public static class SortOrderExtensions
{
    public static SortOrder Reverse(this SortOrder sortOrder)
    {
        return sortOrder == SortOrder.Ascending
            ? SortOrder.Descending
            : SortOrder.Ascending;
    }
}