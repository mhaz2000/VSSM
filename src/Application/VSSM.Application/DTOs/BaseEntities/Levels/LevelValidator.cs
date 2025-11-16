using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.Levels;

public class LevelValidator : AbstractValidator<LevelDto>
{
    public LevelValidator()
    {
        RuleFor(x => x.ShipCount)
            .GreaterThanOrEqualTo(0).WithMessage("تعداد ارسال باید بزرگتر یا مساوی صفر باشد")
            .When(x => x.ShipCount.HasValue);

        RuleFor(x => x.SafetyStockPercentage)
            .InclusiveBetween(0, 100).WithMessage("درصد موجودی ایمنی باید بین 0 تا 100 باشد")
            .When(x => x.SafetyStockPercentage.HasValue);

        RuleFor(x => x.SafetyStockMonth)
            .GreaterThanOrEqualTo(0).WithMessage("مقدار ماه موجودی ایمنی باید بزرگتر یا مساوی صفر باشد")
            .When(x => x.SafetyStockMonth.HasValue);

        RuleFor(x => x.MaxDay)
            .GreaterThanOrEqualTo(0).WithMessage("حداکثر روز باید بزرگتر یا مساوی صفر باشد")
            .When(x => x.MaxDay.HasValue);
    }
}