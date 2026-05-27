using EMP.Web.Services.IServices;
using Newtonsoft.Json;
using static Emp.Web.Utility.SD;
using System.Net.Http.Headers;
using System.Text;
//using Emp.Api.Dtos;
using EMP.Web.Models.Dtos;
using Emp.Web.Models.Dtos;

namespace EMP.Web.Services
{

    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<BaseService> _logger;
        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider, ILogger<BaseService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MainApi");
                HttpRequestMessage message = new HttpRequestMessage();
                message.RequestUri = new Uri(requestDto.Url);

                var token = _tokenProvider.GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

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
                var apiContent = await apiResponse.Content.ReadAsStringAsync();

                // Try to deserialize the API's ResponseDto regardless of status — most non-200 paths
                // (BadRequest, Unauthorized, etc.) still carry a meaningful Message we want to surface.
                ResponseDto? apiResponseDto = null;
                if (!string.IsNullOrWhiteSpace(apiContent))
                {
                    try
                    {
                        apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogWarning(jsonEx,
                            "Failed to deserialize API response from {Url}. Status: {Status}. Body: {Body}",
                            requestDto.Url, apiResponse.StatusCode, apiContent);
                    }
                }

                if (apiResponseDto is not null)
                {
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "API call to {Url} returned {Status}: {Message}",
                            requestDto.Url, (int)apiResponse.StatusCode, apiResponseDto.Message);
                    }
                    return apiResponseDto;
                }

                // Fallback when the body wasn't a ResponseDto (e.g. unhandled 500 HTML page).
                var fallbackMessage = apiResponse.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "Not Found",
                    System.Net.HttpStatusCode.Forbidden => "Access Denied",
                    System.Net.HttpStatusCode.Unauthorized => "Unauthorized",
                    System.Net.HttpStatusCode.InternalServerError => "Internal Server Error",
                    _ => $"HTTP {(int)apiResponse.StatusCode}"
                };
                _logger.LogWarning(
                    "API call to {Url} returned {Status} with non-JSON body. Length: {Length}",
                    requestDto.Url, (int)apiResponse.StatusCode, apiContent?.Length ?? 0);
                return new ResponseDto { IsSuccess = false, Message = fallbackMessage };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API: {Url}", requestDto.Url);
                return new ResponseDto { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
