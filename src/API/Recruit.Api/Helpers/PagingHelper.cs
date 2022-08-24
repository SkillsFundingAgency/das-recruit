using System;

namespace SFA.DAS.Recruit.Api.Helpers
{
    public static class PagingHelper
    {
        public static int GetTotalNoOfPages(int pageSize, int noOfTotalResults)
        {
            return noOfTotalResults == 0
                    ? 0
                    : (int)Math.Ceiling(noOfTotalResults / (double)pageSize);
        }

        public static int GetRequestedPageNo(int pageNo, int pageSize, int noOfTotalResults)
        {
            return Math.Min(pageNo, GetTotalNoOfPages(pageSize, noOfTotalResults));
        }
    }
}