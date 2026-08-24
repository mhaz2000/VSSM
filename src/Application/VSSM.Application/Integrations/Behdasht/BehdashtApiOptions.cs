namespace VSSM.Application.Integrations.Behdasht;

public class BehdashtApiOptions
{
    public string TokenUrl { get; set; } = string.Empty;
    public string CatalogUrl { get; set; } = string.Empty;
    public string BasicAuthorization { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Pid { get; set; } = string.Empty;
    public int TokenExpiryBufferSeconds { get; set; } = 30;
}
