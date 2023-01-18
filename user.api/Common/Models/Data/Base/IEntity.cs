namespace Common.Models.Data;

public interface IEntity
{
    Guid id { get; set; }
    int created_on { get; set; }
}

