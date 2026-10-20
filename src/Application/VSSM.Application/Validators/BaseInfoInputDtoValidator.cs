using FluentValidation;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.Settings;

namespace VSSM.Application.Validators;

public class BaseInfoInputDtoValidator : AbstractValidator<BaseInfoInputDto>
{
    public BaseInfoInputDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Code)
            .Must(c => int.TryParse(c, out _))
            .When(x => SecuritySettings.IsSecurityTitle(x.Title))
            .WithMessage("مقدار تنظیمات امنیتی باید عدد باشد.");
    }
}