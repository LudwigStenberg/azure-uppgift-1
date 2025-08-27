public class VisitorModel
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public DateTime Timestamp = DateTime.UtcNow;
}