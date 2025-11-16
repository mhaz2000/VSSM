using System.ComponentModel.DataAnnotations;

public class OwnershipViewModel
{
    public int? OwnershipId { get; set; }
    [Required(ErrorMessage = "کد نمی‌تواند خالی باشد.")]
    public string Code { get; set; }
    [Required(ErrorMessage = "نام نمی‌تواند خالی باشد.")]
    public string FullName { get; set; }
    public bool IsActive { get; set; }
}
