namespace LibraryManagementSystem.DTOs.Fine
{
    public class FineResponseDTO
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime PaidDate { get; set; }
        public bool IsPaid { get; set; }
        public string UserId { get; set; }
        public string BookId { get; set; }
        public string ReservationId { get; set; }
    }
}
