using Esfa.Recruit.Shared.Web.Helpers;

namespace Esfa.Recruit.Shared.Web.ViewModels;

public class PaginationViewModel
{
    public string Caption { get; }
    public int CurrentPage { get; }
    public int TotalPages { get; }
    public bool Show => TotalPages > 1;

    public PaginationViewModel(int totalItems, int itemsPerPage, int currentPage, string captionFormat)
    {
        CurrentPage = currentPage;
        TotalPages = PagingHelper.GetTotalNoOfPages(itemsPerPage, totalItems);

        var displayStart = ((currentPage - 1) * itemsPerPage) + 1;
        var displayEnd = currentPage == TotalPages ? totalItems : displayStart + itemsPerPage - 1;
        Caption = string.Format(captionFormat, displayStart, displayEnd, totalItems);
    }
}