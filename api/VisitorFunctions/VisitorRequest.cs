using System.ComponentModel.DataAnnotations;

public class VisitorRequest
{
    [Required]
    [StringLength(30, ErrorMessage = "FirstName must contain 1 to 30 characters.")]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(30, ErrorMessage = "LastName must contain 1 to 30 characters.")]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "EmailAddress must have a valid format.")]
    public required string EmailAddress { get; set; }
}