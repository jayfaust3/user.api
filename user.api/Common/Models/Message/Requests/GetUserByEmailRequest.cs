namespace Common.Models.Message.Requests;

public class GetUserByEmailRequest
{
	public GetUserByEmailRequest(string email)
	{
        Email = email;
    }

    public string Email { get; }
}

