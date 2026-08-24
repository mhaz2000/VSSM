using System.Text.Json.Serialization;

namespace VSSM.Application.Integrations.Behdasht;

public class BehdashtTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("scope")] public string Scope { get; set; } = string.Empty;
}

public class BehdashtCatalogRequest
{
    [JsonPropertyName("Barcodeuid")] public string BarcodeUid { get; set; } = string.Empty;
}

public class BehdashtCatalogResponse
{
    [JsonPropertyName("result")] public CatalogResult? Result { get; set; }
    [JsonPropertyName("status")] public CatalogStatus? Status { get; set; }
}

public class CatalogResult
{
    [JsonPropertyName("data")] public CatalogData? Data { get; set; }
}

public class CatalogData
{
    [JsonPropertyName("data")] public CatalogProduct? Product { get; set; }
    [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
    [JsonPropertyName("statusMessage")] public string StatusMessage { get; set; } = string.Empty;
}

public class CatalogProduct
{
    [JsonPropertyName("Manufacturing")] public string Manufacturing { get; set; } = string.Empty;
    [JsonPropertyName("Expiration")] public string Expiration { get; set; } = string.Empty;
    [JsonPropertyName("BatchCode")] public string BatchCode { get; set; } = string.Empty;
    [JsonPropertyName("GenericName")] public string GenericName { get; set; } = string.Empty;
    [JsonPropertyName("GenericCode")] public int GenericCode { get; set; }
    [JsonPropertyName("UID")] public string Uid { get; set; } = string.Empty;
    [JsonPropertyName("GTIN")] public string Gtin { get; set; } = string.Empty;
    [JsonPropertyName("IRC")] public string Irc { get; set; } = string.Empty;
    [JsonPropertyName("LicenseOwner")] public string LicenseOwner { get; set; } = string.Empty;
    [JsonPropertyName("EnglishProductName")] public string EnglishProductName { get; set; } = string.Empty;
    [JsonPropertyName("PersianProductName")] public string PersianProductName { get; set; } = string.Empty;
    [JsonPropertyName("ProductCategory")] public string ProductCategory { get; set; } = string.Empty;
    [JsonPropertyName("ProductCategoryCode")] public int ProductCategoryCode { get; set; }
    [JsonPropertyName("PackageCount")] public int PackageCount { get; set; }
    [JsonPropertyName("StatusCode")] public int StatusCode { get; set; }
    [JsonPropertyName("BarcodeLevel")] public int BarcodeLevel { get; set; }
}

public class CatalogStatus
{
    [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}
