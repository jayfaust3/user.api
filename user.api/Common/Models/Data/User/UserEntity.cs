namespace Common.Models.Data;

public class UserEntity : BaseEntity
{
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email_address { get; set; }
}

