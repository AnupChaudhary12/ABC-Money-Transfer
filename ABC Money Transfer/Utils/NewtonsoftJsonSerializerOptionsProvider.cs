using Newtonsoft.Json.Serialization;

namespace ABC_Money_Transfer.Utils
{
    public static class NewtonsoftJsonSerializerOptionsProvider
    {
        public static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
