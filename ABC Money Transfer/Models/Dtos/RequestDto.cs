using System.Security.AccessControl;

namespace ABC_Money_Transfer.Models.Dtos
{
    public class RequestDto
    {
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string Url { get; set; } = default!;
        public object? Data { get; set; }
    }
}
