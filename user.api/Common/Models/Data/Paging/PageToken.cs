using Common.Models.DTO;

namespace Common.Models.Data;

public class PageToken
{
	public string Term { get; set; }
	public int Cursor { get; set; }
	public int Limit { get; set; }
}

