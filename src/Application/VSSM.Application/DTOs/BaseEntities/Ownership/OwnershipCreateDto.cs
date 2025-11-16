public record OwnershipCreateDto
{
    public string Code { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
}
