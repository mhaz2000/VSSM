namespace VSSM.Application.DTOs.BaseEntities.Dispatches;

public class DispatchArrivedDto
{
    public int? Frzindc { get; set; }
    public int? FrzindcNok { get; set; }

    public List<DispatchArrivedLineDto> Lines { get; set; }
}