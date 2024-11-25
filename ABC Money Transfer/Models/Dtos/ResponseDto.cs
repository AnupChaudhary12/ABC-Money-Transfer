namespace ABC_Money_Transfer.Models.Dtos
{
    public class ResponseDto
    {
        [JsonProperty("data")]
        public ResponseData? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public T? GetResult<T>()
        {
            return Result != null ? JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(Result)) : default;
        }
    }

    public class ResponseData
    {
        [JsonProperty("payload")]
        public List<Payload> Payload { get; set; } = new List<Payload>();
    }

    public class Payload
    {
        public string Date { get; set; }
        public List<Rate> Rates { get; set; } = new List<Rate>();
    }

    public class Rate
    {
        public Currency Currency { get; set; } = new Currency();
        public string Buy { get; set; }
        public string Sell { get; set; }
    }

    public class Currency
    {
        public string Iso3 { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Unit { get; set; }
    }
}
