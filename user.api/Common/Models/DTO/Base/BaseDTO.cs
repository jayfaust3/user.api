namespace Common.Models.DTO;

public abstract class BaseDTO : IDTO
{
    public Guid? Id { get; set; }
    public int? CreatedOn { get; set; }
}

