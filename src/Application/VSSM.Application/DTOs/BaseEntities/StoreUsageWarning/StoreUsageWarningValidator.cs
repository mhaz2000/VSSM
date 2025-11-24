using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.StoreUsageWarning;

public class StoreUsageWarningValidator : AbstractValidator<StoreUsageWarningDto>
{
    public StoreUsageWarningValidator()
    {
        RuleFor(x => x.Dry)
            .InclusiveBetween(0, 100)
            .WithMessage("مقدار Dry باید بین 0 تا 100 باشد");

        RuleFor(x => x.Bz)
            .InclusiveBetween(0, 100)
            .WithMessage("مقدار Bz باید بین 0 تا 100 باشد");

        RuleFor(x => x.Az)
            .InclusiveBetween(0, 100)
            .WithMessage("مقدار Az باید بین 0 تا 100 باشد");

        RuleFor(x => x)
            .Must(x => ((x.Dry) + (x.Bz) + (x.Az)) <= 100)
            .WithMessage("جمع Dry، Bz و Az نباید بیشتر از 100 باشد");
    }
}