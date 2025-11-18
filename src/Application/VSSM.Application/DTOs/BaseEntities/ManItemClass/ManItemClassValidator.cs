using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.ManItemClass;

public class ManItemClassValidator : AbstractValidator<ManItemClassDto>
{
    public ManItemClassValidator()
    {
        RuleFor(x => x.ItemClassCode)
            .NotNull().WithMessage("ItemClassCode is required.");

        RuleFor(x => x.ManufacturerId)
            .NotNull().WithMessage("ManufacturerId is required.")
            .GreaterThan(0).WithMessage("ManufacturerId must be valid.");
    }
}