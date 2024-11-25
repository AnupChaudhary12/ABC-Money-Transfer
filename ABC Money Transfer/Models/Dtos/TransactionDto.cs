﻿namespace ABC_Money_Transfer.Models.Dtos
{
    public class TransactionDto
    {
        public string? TransactionId { get; set; } = default!;
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
        public decimal TransferAmountMYR { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal PayoutAmount { get; set; }
        public string BankName { get; set; } = default!;
        public string AccountNumber { get; set; } = default!;
    }
}
