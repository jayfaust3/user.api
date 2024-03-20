namespace Common.Models.Data;

public abstract class BaseEntity : IEntity
{
    public Guid id { get; set; }
    public long created_on { get; set; }
    public long updated_on { get; set; }
    public Guid? created_by { get; set; }
    public Guid? updated_by { get; set; }
}

