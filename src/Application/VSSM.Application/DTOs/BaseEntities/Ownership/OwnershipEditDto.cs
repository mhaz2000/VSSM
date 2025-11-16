public record OwnershipEditDto
{
    public int OwnershipId { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
}
