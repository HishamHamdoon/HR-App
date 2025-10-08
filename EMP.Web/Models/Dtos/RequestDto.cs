using Emp.Api.Utility;
using static Emp.Web.Utility.SD;

namespace EMP.Web.Models.Dtos
{
    public class RequestDto
    {
        public ApiType ApiType { get; set; } = ApiType.Get;
        public string Url { get; set; }
        public object Data { get; set; }
        public string AccessToekn { get; set; }
        public ContentType ContentType { get; set; } = ContentType.Json;
    }
}
