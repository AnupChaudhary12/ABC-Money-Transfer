
namespace ABC_Money_Transfer.Repositories.Interfaces
{
    public interface IBaseService
    {
        Task<ResponseDto> SendAsync(RequestDto requestDto);
    }
}
