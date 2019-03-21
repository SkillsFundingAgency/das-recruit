using System;

namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class PagerViewModel
    {
        public string Caption { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }

        public PagerViewModel(int totalItems, int itemsPerPage, int currentPage, string captionFormat)
        {
            CurrentPage = currentPage;

            TotalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            var displayStart = ((currentPage - 1) * itemsPerPage) + 1;
            
            var displayEnd = currentPage == TotalPages ? totalItems : displayStart + itemsPerPage - 1;

            Caption = string.Format(captionFormat, displayStart, displayEnd, totalItems);
        }

        public bool ShowPager => TotalPages > 1;
        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;
        public int PreviousPage => CurrentPage - 1;
        public int NextPage => CurrentPage + 1;
    }
}
