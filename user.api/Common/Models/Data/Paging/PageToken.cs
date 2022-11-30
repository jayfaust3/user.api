using Common.Models.DTO;

namespace Common.Models.Data;

public class PageToken<TData> where TData : class, IDTO
{
	public TData EntityLike { get; set; }
	public int Cursor { get; set; }
	public int Limit { get; set; }
}

