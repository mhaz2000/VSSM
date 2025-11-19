using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.Manufacturer;

public class ManufacturerDtoValidator : AbstractValidator<ManufacturerDto>
{
    public ManufacturerDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("وارد کردن کد الزامی است") 
            .MaximumLength(50)
            .WithMessage("کد نباید بیشتر از 50 کاراکتر باشد");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("وارد کردن نام کامل الزامی است") 
            .MaximumLength(255)
            .WithMessage("نام کامل نباید بیشتر از 255 کاراکتر باشد"); 

    }
}