using ABC_Money_Transfer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Tls;

namespace ABC_Money_Transfer.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly DatabaseContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(IExchangeRateService exchangeRateService, DatabaseContext context)
        {
            _exchangeRateService = exchangeRateService;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ShowAllCurrentExchangeRate(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today;
            endDate ??= DateTime.Today;

            var response = await _exchangeRateService.GetExchangeRateAsync(1, 5, startDate.Value, endDate.Value);

            if (!response.IsSuccess || response.Result == null)
            {
                ModelState.AddModelError("", "Could not retrieve exchange rate.");
                return View();
            }

            return View(response);
        }



        [HttpGet]
        public async Task<IActionResult> CreateTransaction()
        {
            return View(new TransactionDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(TransactionDto transactionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(transactionDto);
                }
                var startDate = DateTime.Today;
                var endDate = DateTime.Today;
                var response = await _exchangeRateService.GetExchangeRateAsync(1, 5, startDate, endDate);

                if (!response.IsSuccess || response.Result == null)
                {
                    ModelState.AddModelError("", "Could not retrieve exchange rate.");
                    return View(transactionDto);
                }
                string senderCountryIso3 = GetCountryIso3(transactionDto.SenderCountry);
                decimal exchangeRate = GetExchangeRate(response, senderCountryIso3);

                transactionDto.ExchangeRate = exchangeRate;
                transactionDto.PayoutAmount = transactionDto.TransferAmountMYR * exchangeRate;
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? throw new InvalidOperationException("User is not authenticated.");
                var transaction = new Transaction
                {
                    TransactionId = DateTime.Now.ToFileTime().ToString(),
                    SenderFirstName = transactionDto.SenderFirstName,
                    SenderMiddleName = transactionDto.SenderMiddleName,
                    SenderLastName = transactionDto.SenderLastName,
                    SenderAddress = transactionDto.SenderAddress,
                    SenderCountry = transactionDto.SenderCountry,
                    ReceiverFirstName = transactionDto.ReceiverFirstName,
                    ReceiverMiddleName = transactionDto.ReceiverMiddleName,
                    ReceiverLastName = transactionDto.ReceiverLastName,
                    ReceiverAddress = transactionDto.ReceiverAddress,
                    ReceiverCountry = transactionDto.ReceiverCountry,
                    TransferAmount = transactionDto.TransferAmountMYR,
                    ExchangeRate = transactionDto.ExchangeRate,
                    PayoutAmount = transactionDto.PayoutAmount,
                    BankName = transactionDto.BankName,
                    AccountNumber = transactionDto.AccountNumber,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = transaction.TransactionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(transactionDto);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }


        private string GetCountryIso3(string countryName) =>
     countryName switch
     {
         "India" => "INR",
         "UnitedStates" => "USD",
         "EuropeanUnion" => "EUR",
         "UnitedKingdom" => "GBP",
         "Switzerland" => "CHF",
         "Australia" => "AUD",
         "Canada" => "CAD",
         "Singapore" => "SGD",
         "Japan" => "JPY",
         "China" => "CNY",
         "Saudi Arabia" => "SAR",
         "Qatar" => "QAR",
         "Thailand" => "THB",
         "UAE" => "AED",
         "Malaysia" => "MYR",
         "South Korea" => "KRW",
         "Sweden" => "SEK",
         "Denmark" => "DKK",
         "Hong Kong" => "HKD",
         "Kuwait" => "KWD",
         "Bahrain" => "BHD",
         "Oman" => "OMR",
         _ => throw new ArgumentException("Invalid country name.")
     };


        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GenerateReport(DateTime? startDate, DateTime? endDate, string senderName, string receiverName, string senderCountry)
        {
            if (endDate == null)
            {
                endDate = DateTime.Now;
            }

            if (startDate != null && endDate != null && startDate.Value.Date == endDate.Value.Date)
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(senderName))
            {
                query = query.Where(t => (senderName.Contains(t.SenderFirstName) || senderName.Contains(t.SenderLastName)));
            }

            if (!string.IsNullOrEmpty(receiverName))
            {
                query = query.Where(t => (receiverName.Contains(t.ReceiverFirstName) || receiverName.Contains(t.ReceiverLastName)));
            }

            if (!string.IsNullOrEmpty(senderCountry))
            {
                query = query.Where(t => t.SenderCountry.Contains(senderCountry));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
            }

            var transactions = await query.ToListAsync();

            var transactionDtos = transactions.Select(t => new GetTransactionDto
            {
                TransactionId = t.TransactionId,
                SenderFirstName = t.SenderFirstName,
                SenderMiddleName = t.SenderMiddleName,
                SenderLastName = t.SenderLastName,
                SenderAddress = t.SenderAddress,
                SenderCountry = t.SenderCountry,
                ReceiverFirstName = t.ReceiverFirstName,
                ReceiverMiddleName = t.ReceiverMiddleName,
                ReceiverLastName = t.ReceiverLastName,
                ReceiverAddress = t.ReceiverAddress,
                ReceiverCountry = t.ReceiverCountry,
                TransferAmount = t.TransferAmount,
                ExchangeRate = t.ExchangeRate,
                PayoutAmount = t.PayoutAmount,
                BankName = t.BankName,
                AccountNumber = t.AccountNumber,
                CreatedAt = t.CreatedAt
            }).ToList();

            return View(transactionDtos);
        }


        [HttpPost]
        public async Task<IActionResult> GenerateExcelReport(DateTime? startDate, DateTime? endDate, string senderName, string receiverName, string senderCountry)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(senderName))
            {
                query = query.Where(t => (t.SenderFirstName.Contains(senderName) || t.SenderLastName.Contains(senderName)));
            }

            if (!string.IsNullOrEmpty(receiverName))
            {
                query = query.Where(t => (t.ReceiverFirstName.Contains(receiverName) || t.ReceiverLastName.Contains(receiverName)));
            }

            if (!string.IsNullOrEmpty(senderCountry))
            {
                query = query.Where(t => t.SenderCountry.Contains(senderCountry));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
            }

            var transactions = await query.ToListAsync();

            var transactionDtos = transactions.Select(t => new GetTransactionDto
            {
                TransactionId = t.TransactionId,
                SenderFirstName = t.SenderFirstName,
                SenderMiddleName = t.SenderMiddleName,
                SenderLastName = t.SenderLastName,
                SenderAddress = t.SenderAddress,
                SenderCountry = t.SenderCountry,
                ReceiverFirstName = t.ReceiverFirstName,
                ReceiverMiddleName = t.ReceiverMiddleName,
                ReceiverLastName = t.ReceiverLastName,
                ReceiverAddress = t.ReceiverAddress,
                ReceiverCountry = t.ReceiverCountry,
                TransferAmount = t.TransferAmount,
                ExchangeRate = t.ExchangeRate,
                PayoutAmount = t.PayoutAmount,
                BankName = t.BankName,
                AccountNumber = t.AccountNumber,
                CreatedAt = t.CreatedAt
            }).ToList();

            var fileContent = GenerateExcel(transactionDtos);

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TransactionReport.xlsx");
        }
        private byte[] GenerateExcel(List<GetTransactionDto> transactions)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Transactions");

            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Transaction ID");
            headerRow.CreateCell(1).SetCellValue("Sender Name");
            headerRow.CreateCell(2).SetCellValue("Receiver Name");
            headerRow.CreateCell(3).SetCellValue("Transfer Amount");
            headerRow.CreateCell(4).SetCellValue("Exchange Rate");
            headerRow.CreateCell(5).SetCellValue("Payout Amount");
            headerRow.CreateCell(6).SetCellValue("Sender Country");
            headerRow.CreateCell(7).SetCellValue("Created At");

            int row = 1;
            foreach (var transaction in transactions)
            {
                var dataRow = sheet.CreateRow(row);
                dataRow.CreateCell(0).SetCellValue(transaction.TransactionId);
                dataRow.CreateCell(1).SetCellValue($"{transaction.SenderFirstName} {transaction.SenderLastName}");
                dataRow.CreateCell(2).SetCellValue($"{transaction.ReceiverFirstName} {transaction.ReceiverLastName}");
                dataRow.CreateCell(3).SetCellValue((double)transaction.TransferAmount);
                dataRow.CreateCell(4).SetCellValue((double)transaction.TransferAmount);
                dataRow.CreateCell(5).SetCellValue((double)transaction.TransferAmount);
                dataRow.CreateCell(6).SetCellValue(transaction.SenderCountry);
                dataRow.CreateCell(7).SetCellValue(transaction.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                row++;
            }

            using (var memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public decimal GetExchangeRate(ResponseDto responseDto, string currencyCode)
        {
            var exchangeRateData = responseDto?.Result?.Payload?.FirstOrDefault();
            if (exchangeRateData == null)
            {
                throw new Exception("No exchange rate data available.");
            }

            var rate = exchangeRateData.Rates?.FirstOrDefault(r => r.Currency.Iso3 == currencyCode);
            if (rate == null)
            {
                throw new Exception($"Exchange rate for {currencyCode} is not available.");
            }

            if (decimal.TryParse(rate.Sell, out decimal rateValue))
            {
                return rateValue;
            }
            else
            {
                throw new Exception($"Failed to parse exchange rate: {rate.Sell}");
            }
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> GenerateUserReport(DateTime? startDate, DateTime? endDate, string senderName, string receiverName, string senderCountry)
        {
            if (endDate == null)
            {
                endDate = DateTime.Now;
            }

            if (startDate != null && endDate != null && startDate.Value.Date == endDate.Value.Date)
            {
                endDate = endDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                ?? throw new InvalidOperationException("User is not authenticated.");
            if (userId == null)
            {
                return NotFound("User not found.");
            }

            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(senderName))
            {
                query = query.Where(t => (senderName.Contains(t.SenderFirstName) || senderName.Contains(t.SenderLastName)));
            }

            if (!string.IsNullOrEmpty(receiverName))
            {
                query = query.Where(t => (receiverName.Contains(t.ReceiverFirstName) || receiverName.Contains(t.ReceiverLastName)));
            }

            if (!string.IsNullOrEmpty(senderCountry))
            {
                query = query.Where(t => t.SenderCountry.Contains(senderCountry));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value && t.CreatedAt <= endDate.Value);
            }

            query = query.Where(t => t.UserId == userId);

            var transactions = await query.ToListAsync();

            var transactionDtos = transactions.Select(t => new GetTransactionDto
            {
                TransactionId = t.TransactionId,
                SenderFirstName = t.SenderFirstName,
                SenderMiddleName = t.SenderMiddleName,
                SenderLastName = t.SenderLastName,
                SenderAddress = t.SenderAddress,
                SenderCountry = t.SenderCountry,
                ReceiverFirstName = t.ReceiverFirstName,
                ReceiverMiddleName = t.ReceiverMiddleName,
                ReceiverLastName = t.ReceiverLastName,
                ReceiverAddress = t.ReceiverAddress,
                ReceiverCountry = t.ReceiverCountry,
                TransferAmount = t.TransferAmount,
                ExchangeRate = t.ExchangeRate,
                PayoutAmount = t.PayoutAmount,
                BankName = t.BankName,
                AccountNumber = t.AccountNumber,
                CreatedAt = t.CreatedAt
            }).ToList();

            return View(transactionDtos);
        }

    }
}
