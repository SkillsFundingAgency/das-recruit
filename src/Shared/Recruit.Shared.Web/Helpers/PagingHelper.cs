using System;

namespace Esfa.Recruit.Shared.Web.Helpers
{
    public static class PagingHelper
    {
        public static int GetPageNoOfSelectedItem(int totalNumberOfPages, int maxResultsPerPage, int nonZeroIndexOfSelectedItem)
        {
            int page;
            var pageNoOfSelectedItem = (int)Math.Floor((nonZeroIndexOfSelectedItem - 1) / (double)maxResultsPerPage) + 1;

            if (pageNoOfSelectedItem == totalNumberOfPages)
            {
                page = totalNumberOfPages;
            }
            else if (pageNoOfSelectedItem > totalNumberOfPages)
            {
                page = totalNumberOfPages;
            }
            else
            {
                page = pageNoOfSelectedItem;
            }

            return page;
        }
    }
}