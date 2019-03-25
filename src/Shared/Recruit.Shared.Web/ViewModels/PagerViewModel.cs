using System;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class PagerViewModel
    {
        public string Caption { get; }
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public string RouteName { get; }
        public Dictionary<string,string> OtherRouteValues { get; }

        public PagerViewModel(int totalItems, int itemsPerPage, int currentPage, string captionFormat, string routeName, Dictionary<string, string> otherRouteValues = null)
        {
            CurrentPage = currentPage;

            TotalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            var displayStart = ((currentPage - 1) * itemsPerPage) + 1;
            
            var displayEnd = currentPage == TotalPages ? totalItems : displayStart + itemsPerPage - 1;

            Caption = string.Format(captionFormat, displayStart, displayEnd, totalItems);

            RouteName = routeName;

            OtherRouteValues = otherRouteValues ?? new Dictionary<string, string>();
        }

        public Dictionary<string, string> GetRouteData(int page)
        {
            return new Dictionary<string, string>(OtherRouteValues) {
                { "page", page.ToString() }
            };
        }

        public bool ShowPager => TotalPages > 1;
        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;
        public Dictionary<string,string> PreviousPageRouteData => GetRouteData(CurrentPage - 1);
        public Dictionary<string, string> NextPageRouteData => GetRouteData(CurrentPage + 1);
    }
}
