namespace LibraryManagementSystem.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; } = null!;
        public string BorrowedBy { get; set; } = string.Empty;   // username
        public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnedAt { get; set; }                 
    }
}
