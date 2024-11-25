namespace ABC_Money_Transfer.Repositories.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ResponseDto> GetExchangeRateAsync(int page, int per_page, DateTime? from, DateTime? to);
        Task<ResponseDto> GetExchangeRateAsync( string fromCurrency, string toCurrency);

    }
}
