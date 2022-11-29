namespace Common.Models.Data;

public abstract class BaseEntity : IEntity
{
    public Guid id { get; set; }
    public long created_on { get; set; }
}

