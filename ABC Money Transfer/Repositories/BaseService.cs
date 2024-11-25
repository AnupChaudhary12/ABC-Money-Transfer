

namespace ABC_Money_Transfer.Repositories
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BaseService> _logger;
        private readonly JsonSerializerSettings _settings;
        private readonly ApiSettings _options;

        public BaseService(IHttpClientFactory httpClientFactory, ILogger<BaseService> logger, JsonSerializerSettings settings, IOptions<ApiSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings;
            _options = options.Value;
        }

        public async Task<ResponseDto> SendAsync(RequestDto requestDto)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();

                string requestUri = $"{_options.ApiBaseUrl}{requestDto.Url}";
                HttpRequestMessage message = new HttpRequestMessage
                {
                    RequestUri = new Uri(requestUri)
                };
                message.Headers.Add("Accept", "*/*");
                message.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                message.Headers.Add("Connection", "keep-alive");

                switch (requestDto.ApiType)
                {
                    case SD.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case SD.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case SD.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }
                if (requestDto.Data != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
                }


                HttpResponseMessage apiResponse = await client.SendAsync(message);


                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new ResponseDto { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.BadRequest:
                        return new ResponseDto { IsSuccess = false, Message = "Bad Request" };
                    case HttpStatusCode.Unauthorized:
                        return new ResponseDto { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.Forbidden:
                        return new ResponseDto { IsSuccess = false, Message = "Forbidden" };
                    case HttpStatusCode.InternalServerError:
                        return new ResponseDto { IsSuccess = false, Message = "Internal Server Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();

                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);

                        if (apiResponseDto == null)
                        {
                            return new ResponseDto { IsSuccess = false, Message = "Failed to parse the API response" };
                        }

                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                return new ResponseDto { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
