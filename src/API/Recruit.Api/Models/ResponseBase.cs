using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Models
{
    public abstract class ResponseBase
    {
        public ResponseCode ResultCode { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();

        public object Data { get; set; }
    }

    public enum ResponseCode
    {
        Success,
        InvalidRequest,
        NotFound
    }
}