using System.Collections.Generic;

namespace SFA.DAS.Recruit.Api.Models
{
    public abstract class ResponseBase
    {
        public ResponseCode ResultCode { get; set; }
        public List<object> ValidationErrors { get; set; } = new List<object>();
        public object Data { get; set; }
    }

    public enum ResponseCode
    {
        Success,
        InvalidRequest,
        NotFound,
        Created
    }

    public class DetailedValidationError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}