using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VSSM.Domain.ValueObjects;

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
        BehdashtCatalogResponse? response;
        try
        {
            response = await PostBearerAsync<BehdashtCatalogRequest, BehdashtCatalogResponse>(
                _options.CatalogUrl, new BehdashtCatalogRequest { BarcodeUid = secUid }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Behdasht catalog validation failed for UID {Uid}", secUid);
            return false;
        }

        return response?.Status == null || response.Status.StatusCode == 200;
    }

    public async Task<BehdashtSearchOrganizationsResponse?> SearchOrganizationsAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        return await PostBearerAsync<BehdashtSearchOrganizationsRequest, BehdashtSearchOrganizationsResponse>(
            _options.SearchOrganizationsUrl,
            new BehdashtSearchOrganizationsRequest { Data = new SearchPageData { PageNumber = pageNumber, PageSize = pageSize } },
            ct);
    }

    public async Task<BehdashtGetOrganizationResponse?> GetOrganizationAsync(string id, CancellationToken ct)
    {
        return await PostBearerAsync<BehdashtGetOrganizationRequest, BehdashtGetOrganizationResponse>(
            _options.GetOrganizationUrl,
            new BehdashtGetOrganizationRequest { Data = new GetOrgIdentifierData { Identifier = new OrgRef { Id = id } } },
            ct);
    }

    private async Task<TResponse> PostBearerAsync<TRequest, TResponse>(string url, TRequest request, CancellationToken ct)
        where TResponse : class
    {
        var token = await GetValidTokenAsync(ct);
        var client = _httpClientFactory.CreateClient("BehdashtApi");

        var response = await SendBearerAsync(client, token, url, request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Behdasht token rejected (401) for {Url}. Refreshing token and retrying", url);
            token = await RefreshTokenAsync(ct);
            response = await SendBearerAsync(client, token, url, request, ct);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Behdasht API {Url} returned HTTP {StatusCode}: {Body}", url, (int)response.StatusCode, body);
            throw new InvalidOperationException($"Behdasht API {url} returned HTTP {(int)response.StatusCode}");
        }

        var raw = await response.Content.ReadAsStringAsync(ct);
        var deserialized = JsonSerializer.Deserialize<TResponse>(raw, JsonOptions);
        if (deserialized == null)
            throw new InvalidOperationException($"Behdasht API {url} returned an empty/non-deserializable body");

        return deserialized;
    }

    private async Task<HttpResponseMessage> SendBearerAsync(HttpClient client, string token, string url, object request, CancellationToken ct)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.TryAddWithoutValidation("pid", _options.Pid);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.SendAsync(httpRequest, ct);
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

        var token = JsonSerializer.Deserialize<BehdashtTokenResponse>(await response.Content.ReadAsStringAsync(ct), JsonOptions);
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
}
