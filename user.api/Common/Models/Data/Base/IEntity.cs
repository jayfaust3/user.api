namespace Common.Models.Data;

public interface IEntity
{
    Guid id { get; set; }
    long created_on { get; set; }
}

