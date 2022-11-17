namespace Common.Models.API;

public class APIResponse<TData>
{
    public APIResponse(TData? data, string? message = null)
    {
        Data = data;
        Message = message;
    }
    public TData? Data { get; }
    public string? Message { get; }
}
