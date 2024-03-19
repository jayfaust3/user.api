namespace Common.Models.API;

public class APIResponse<TData> where TData : class?
{
    public APIResponse(TData? data = null, string? message = null)
    {
        Data = data;
        Message = message;
    }
    public TData? Data { get; }
    public string? Message { get; }
}
