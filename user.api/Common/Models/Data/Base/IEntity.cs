namespace Common.Models.Data;

public interface IEntity
{
    Guid id { get; set; }
    long created_on { get; set; }
    long updated_on { get; set; }
    Guid? created_by { get; set; }
    Guid? updated_by { get; set; }
}

