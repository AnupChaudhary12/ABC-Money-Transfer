namespace ABC_Money_Transfer.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = default!;
        public string? ProfilePicture { get; set; } = default!;
        public ICollection<Transaction>? Transactions { get; set; } = new List<Transaction>();
    }
}
