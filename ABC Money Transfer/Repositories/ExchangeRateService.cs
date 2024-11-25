
namespace ABC_Money_Transfer.Repositories
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IBaseService _baseService;
        public ExchangeRateService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto> GetExchangeRateAsync(int page, int per_page, DateTime? from, DateTime? to)
        {
            try
            {
                var formattedFrom = from?.ToString("yyyy-MM-dd");
                var formattedTo = to?.ToString("yyyy-MM-dd");

                var responseDto = await _baseService.SendAsync(new RequestDto
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"rates?page={page}&per_page={per_page}&from={formattedFrom}&to={formattedTo}"
                });

                if (responseDto?.Result == null || responseDto.Result?.Payload == null || !responseDto.Result.Payload.Any())
                {
                    return new ResponseDto { IsSuccess = false, Message = "Exchange rate data is not available" };
                }

                var exchangeRateData = responseDto.Result.Payload.FirstOrDefault();

                if (exchangeRateData == null)
                {
                    return new ResponseDto { IsSuccess = false, Message = "No exchange rate data found" };
                }

                return new ResponseDto { IsSuccess = true, Result = new ResponseData { Payload = new List<Payload> { exchangeRateData } } };
            }
            catch (Exception ex)
            {
                return new ResponseDto { IsSuccess = false, Message = ex.Message.ToString() };
            }
        }

        public async Task<ResponseDto> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            try
            {
                var responseDto = await _baseService.SendAsync(new RequestDto
                {
                    ApiType = SD.ApiType.GET,
                    Url = $"rates"
                });
                if (responseDto?.Result == null)
                {
                    return new ResponseDto { IsSuccess = false, Message = "ExchangeRate is not available" };
                }
                return responseDto;
            }
            catch (Exception ex)
            {
                return new ResponseDto { IsSuccess = false, Message = ex.Message.ToString() };
            }
        }
    }
}
