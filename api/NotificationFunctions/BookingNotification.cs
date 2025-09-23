public class BookingNotification
{
    public int BookingId { get; set; }
    public required string CustomerEmail { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public required string FacilityName { get; set; }
    public int NumberOfParticipants { get; set; }
    public string? BookingNotes { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
}