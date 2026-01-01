namespace VSSM.Application.Shared;

public class ApiResult<TResult>
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public TResult Result { get; set; }
}
