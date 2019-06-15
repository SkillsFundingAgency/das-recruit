using System;

namespace Esfa.Recruit.Shared.Web.Helpers
{
    public static class PagingHelper
    {
        public static int GetTotalNoOfPages(int pageSize, int noOfTotalResults)
        {
            return noOfTotalResults == 0
                    ? 0
                    : (int)Math.Ceiling(noOfTotalResults / (double)pageSize);
        }

        public static int GetPageNoOfSelectedItem(int totalNumberOfPages, int maxResultsPerPage, int nonZeroIndexOfSelectedItem)
        {
            var pageNoOfSelectedItem = (int)Math.Floor((nonZeroIndexOfSelectedItem - 1) / (double)maxResultsPerPage) + 1;

            return pageNoOfSelectedItem >= totalNumberOfPages ? totalNumberOfPages : pageNoOfSelectedItem;
        }
    }
}