namespace Common.Models.Data;

public abstract class BaseEntity : IEntity
{
    public Guid id { get; set; }
    public int created_on { get; set; }
}

