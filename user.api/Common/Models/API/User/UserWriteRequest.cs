using System.ComponentModel.DataAnnotations;

namespace Common.Models.API;

public class UserWriteRequest
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string EmailAddress { get; set; }
}

