using Common.Models.Message;

namespace Common.Exceptions;

public class NotFoundException : Exception
{
	public NotFoundException(string message) : base(message) { }
}

