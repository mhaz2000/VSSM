using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VSSM.Application.Integrations.Behdasht;

public class BehdashtApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BehdashtApiOptions _options;
    private readonly ILogger<BehdashtApiClient> _logger;
    private readonly object _tokenLock = new();
    private string _accessToken = string.Empty;
    private DateTime _tokenExpiryUtc = DateTime.MinValue;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public BehdashtApiClient(IHttpClientFactory httpClientFactory, IOptions<BehdashtApiOptions> options, ILogger<BehdashtApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> ValidateUidAsync(string secUid, CancellationToken ct)
    {
        var token = await GetValidTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("BehdashtApi");

        var response = await SendCatalogAsync(client, token, secUid, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Behdasht token rejected (401). Refreshing token and retrying UID {Uid}", secUid);
            token = await RefreshTokenAsync(ct);
            response = await SendCatalogAsync(client, token, secUid, ct);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Behdasht catalog API returned HTTP {StatusCode} for UID {Uid}", (int)response.StatusCode, secUid);
            return false;
        }

        var payload = await DeserializeAsync<BehdashtCatalogResponse>(response, ct);
        if (payload?.Status != null && payload.Status.StatusCode != 200)
        {
            _logger.LogError("Behdasht catalog API body status {StatusCode} ({Message}) for UID {Uid}", payload.Status.StatusCode, payload.Status.Message, secUid);
            return false;
        }

        return true;
    }

    private async Task<HttpResponseMessage> SendCatalogAsync(HttpClient client, string token, string secUid, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.CatalogUrl)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new BehdashtCatalogRequest { BarcodeUid = secUid }, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
        request.Headers.TryAddWithoutValidation("pid", _options.Pid);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.SendAsync(request, ct);
    }

    private async Task<string> GetValidTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc)
            return _accessToken;

        return await RefreshTokenAsync(ct);
    }

    private async Task<string> RefreshTokenAsync(CancellationToken ct)
    {
        lock (_tokenLock)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc)
                return _accessToken;
        }

        var client = _httpClientFactory.CreateClient("BehdashtApi");
        var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = _options.Username,
                ["password"] = _options.Password
            })
        };
        request.Headers.TryAddWithoutValidation("Authorization", _options.BasicAuthorization);

        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Behdasht token endpoint returned HTTP {StatusCode}: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Failed to obtain Behdasht access token (HTTP {(int)response.StatusCode})");
        }

        var token = await DeserializeAsync<BehdashtTokenResponse>(response, ct);
        if (token == null || string.IsNullOrEmpty(token.AccessToken))
        {
            _logger.LogError("Behdasht token response missing access_token");
            throw new InvalidOperationException("Behdasht token response missing access_token");
        }

        lock (_tokenLock)
        {
            _accessToken = token.AccessToken;
            _tokenExpiryUtc = DateTime.UtcNow.AddSeconds(Math.Max(0, token.ExpiresIn - _options.TokenExpiryBufferSeconds));
        }

        _logger.LogInformation("Behdasht access token obtained (expires in {ExpiresIn}s)", token.ExpiresIn);
        return _accessToken;
    }

    private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body))
            return default;

        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }
}
