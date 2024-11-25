namespace ABC_Money_Transfer.Models.Domain
{
    public class Transaction
    {
        public int Id { get; set; }
        public string? TransactionId { get; set; } =default!;
        public string SenderFirstName { get; set; } = default!;
        public string? SenderMiddleName { get; set; } = default!;
        public string SenderLastName { get; set; } = default!;
        public string SenderAddress { get; set; } = default!;
        public string SenderCountry { get; set; } = default!;
        public string ReceiverFirstName { get; set; } = default!;
        public string? ReceiverMiddleName { get; set; } = default!;
        public string ReceiverLastName { get; set; } = default!;
        public string ReceiverAddress { get; set; } = default!;
        public string ReceiverCountry { get; set; } = default!;
        public decimal TransferAmount { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal PayoutAmount { get; set; }
        public string BankName { get; set; } = default!;
        public string AccountNumber { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = default!;
        public ApplicationUser? User { get; set; } = default!;
    }
}
