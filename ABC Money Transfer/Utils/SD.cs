namespace ABC_Money_Transfer.Utils
{
    public class SD
    {
        public static string ApiBaseUrl { get; set; } = default!;   
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
    }
}
