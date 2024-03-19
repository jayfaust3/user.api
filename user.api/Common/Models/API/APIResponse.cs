namespace Common.Models.API;

public class APIResponse<TData> where TData : class?
{
    public TData? Data { get; }

    public string? Message { get; }

    public APIResponse(TData? data = null, string? message = null)
    {
        Data = data;
        Message = message;
    }
}
