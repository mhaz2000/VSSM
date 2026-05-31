using System.ComponentModel.DataAnnotations;

namespace VSSM.Application.DTOs.BaseEntities.BaseInfo;

public record BaseInfoInputDto
{
    public int? ParentId { get; set; }
    public int? BaseInfoId { get; set; }
    [Required(ErrorMessage = "عنوان الزامی است.")]
    public string Title { get; set; }
    public string? Code { get; set; }
    public int? Order { get; set; }
}