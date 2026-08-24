using System.Text.Json.Serialization;
using VSSM.Domain.ValueObjects;

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

public class BehdashtSearchOrganizationsRequest
{
    [JsonPropertyName("data")] public SearchPageData Data { get; set; } = new();
}

public class SearchPageData
{
    [JsonPropertyName("pageNumber")] public int PageNumber { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

public class BehdashtSearchOrganizationsResponse
{
    [JsonPropertyName("result")] public SearchResultContainer? Result { get; set; }
    [JsonPropertyName("status")] public CatalogStatus? Status { get; set; }
}

public class SearchResultContainer
{
    [JsonPropertyName("data")] public SearchDataContainer? Data { get; set; }
    [JsonPropertyName("status")] public CatalogStatus? Status { get; set; }
}

public class SearchDataContainer
{
    [JsonPropertyName("data")] public SearchInnerData? Data { get; set; }
    [JsonPropertyName("succeeded")] public bool Succeeded { get; set; }
}

public class SearchInnerData
{
    [JsonPropertyName("Items")] public List<SearchItem>? Items { get; set; }
    [JsonPropertyName("TotalCount")] public int TotalCount { get; set; }
    [JsonPropertyName("PageNumber")] public int PageNumber { get; set; }
    [JsonPropertyName("PageSize")] public int PageSize { get; set; }
    [JsonPropertyName("TotalPages")] public int TotalPages { get; set; }
    [JsonPropertyName("HasPreviousPage")] public bool HasPreviousPage { get; set; }
    [JsonPropertyName("HasNextPage")] public bool HasNextPage { get; set; }
}

public class SearchItem
{
    [JsonPropertyName("issuer")] public string? Issuer { get; set; }
    [JsonPropertyName("assigner")] public string? Assigner { get; set; }
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
}

public class BehdashtGetOrganizationRequest
{
    [JsonPropertyName("data")] public GetOrgIdentifierData Data { get; set; } = new();
}

public class GetOrgIdentifierData
{
    [JsonPropertyName("identifier")] public OrgRef Identifier { get; set; } = new();
}

public class BehdashtGetOrganizationResponse
{
    [JsonPropertyName("result")] public GetOrgResultContainer? Result { get; set; }
    [JsonPropertyName("status")] public CatalogStatus? Status { get; set; }
}

public class GetOrgResultContainer
{
    [JsonPropertyName("data")] public GetOrgDataContainer? Data { get; set; }
    [JsonPropertyName("status")] public CatalogStatus? Status { get; set; }
}

public class GetOrgDataContainer
{
    [JsonPropertyName("data")] public OrganizationDetail? Data { get; set; }
    [JsonPropertyName("succeeded")] public bool Succeeded { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
}

public class OrgArea
{
    [JsonPropertyName("nationalAreaCode")] public CodedValue? NationalAreaCode { get; set; }
    [JsonPropertyName("country")] public CodedValue? Country { get; set; }
    [JsonPropertyName("province")] public CodedValue? Province { get; set; }
    [JsonPropertyName("town")] public CodedValue? Town { get; set; }
    [JsonPropertyName("district")] public CodedValue? District { get; set; }
    [JsonPropertyName("ruralArea")] public CodedValue? RuralArea { get; set; }
    [JsonPropertyName("city")] public CodedValue? City { get; set; }
    [JsonPropertyName("village")] public CodedValue? Village { get; set; }
}

public class ExtendedProperty
{
    [JsonPropertyName("element")] public string? Element { get; set; }
    [JsonPropertyName("valueField")] public string? ValueField { get; set; }
}

public class OrganizationDetail
{
    [JsonPropertyName("organizationID")] public OrgRef? OrganizationId { get; set; }
    [JsonPropertyName("hierarchicalOrganizationID")] public OrgRef? HierarchicalOrganizationId { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("type")] public CodedValue? Type { get; set; }
    [JsonPropertyName("area")] public OrgArea? Area { get; set; }
    [JsonPropertyName("dependencyType")] public CodedValue? DependencyType { get; set; }
    [JsonPropertyName("allegianceType")] public CodedValue? AllegianceType { get; set; }
    [JsonPropertyName("ownershipStatus")] public CodedValue? OwnershipStatus { get; set; }
    [JsonPropertyName("ownerOrganizationID")] public OrgRef? OwnerOrganizationId { get; set; }
    [JsonPropertyName("observerOrganizationID")] public OrgRef? ObserverOrganizationId { get; set; }
    [JsonPropertyName("specialty")] public CodedValue? Specialty { get; set; }
    [JsonPropertyName("specialtyType")] public List<CodedValue>? SpecialtyType { get; set; }
    [JsonPropertyName("activityStatus")] public CodedValue? ActivityStatus { get; set; }
    [JsonPropertyName("postalCode")] public string? PostalCode { get; set; }
    [JsonPropertyName("longitude")] public double? Longitude { get; set; }
    [JsonPropertyName("latitude")] public double? Latitude { get; set; }
    [JsonPropertyName("establishmentDate")] public PartialDate? EstablishmentDate { get; set; }
    [JsonPropertyName("activationDate")] public PartialDate? ActivationDate { get; set; }
    [JsonPropertyName("otherIdentityInfo")] public List<OrgRef>? OtherIdentityInfo { get; set; }
    [JsonPropertyName("extendendProperty")] public List<ExtendedProperty>? ExtendedProperty { get; set; }
}
