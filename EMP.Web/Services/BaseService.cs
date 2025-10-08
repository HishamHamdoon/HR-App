using EMP.Web.Services.IServices;
using Newtonsoft.Json;
using static Emp.Web.Utility.SD;
using System.Text;
//using Emp.Api.Dtos;
using EMP.Web.Models.Dtos;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services
{

    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public BaseService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MainApi");
                HttpRequestMessage message = new HttpRequestMessage();
                //token
                message.RequestUri = new Uri(requestDto.Url);

                
                if (requestDto.ContentType ==ContentType.MultiPartFormData)
                {
                    message.Headers.Add("Accept","*/*");
                }
                else
                {
                    message.Headers.Add("Accept", "application/json");
                }
                if (requestDto.ContentType == ContentType.MultiPartFormData)
                {
                    var content = new  MultipartFormDataContent();
                    foreach (var prop in requestDto.Data.GetType().GetProperties())
                    {
                        var value = prop.GetValue(requestDto.Data);
                        if (value is FormFile)
                        {
                            var file = (FormFile)value;
                            if (file!=null)
                            {
                                content.Add(new StreamContent(file.OpenReadStream()),prop.Name,file.FileName);
                            }
                        }
                        else
                        {
                            content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
                        }
                    }
                    message.Content = content;
                }
                else
                {
                    if (requestDto.Data is not null)
                    {
                        message.Content = new StringContent(
                            JsonConvert.SerializeObject(requestDto.Data),
                            Encoding.UTF8,
                            "application/json"
                        );
                    }
                }


                    HttpResponseMessage? apiResponse = null;
                switch (requestDto.ApiType)
                {
                    case ApiType.Post:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.Put:
                        message.Method = HttpMethod.Put;
                        break;
                    case ApiType.Delete:
                        message.Method = HttpMethod.Delete;
                        break;
                    case ApiType.Patch:
                        message.Method = HttpMethod.Patch;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }
                apiResponse = await client.SendAsync(message);
                switch (apiResponse.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        return new ResponseDto() { IsSuccess = false, Message = "Not Found" };
                    case System.Net.HttpStatusCode.Forbidden:
                        return new ResponseDto() { IsSuccess = false, Message = "Access Denied" };
                    case System.Net.HttpStatusCode.Unauthorized:
                        return new ResponseDto() { IsSuccess = false, Message = "Unauthorized" };
                    case System.Net.HttpStatusCode.InternalServerError:
                        return new ResponseDto() { IsSuccess = false, Message = "Internal Server Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto();
                dto.IsSuccess = false;
                dto.Message = ex.Message;
                return dto;

            }
        }
    }
}
