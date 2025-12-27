using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.Adjustments;

public class AdjustmentDtoValidator : AbstractValidator<AdjustmentDto>
{
    public AdjustmentDtoValidator()
    {
        RuleFor(x => x.Note)
            .MaximumLength(500)
            .WithMessage("توضیحات نباید بیشتر از 500 کاراکتر باشد");

        RuleFor(x => x.OldQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مقدار قبلی باید صفر یا بیشتر باشد");

        RuleFor(x => x.NewQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مقدار جدید باید صفر یا بیشتر باشد");

        RuleFor(x => x.AdjustmentreasonId)
            .GreaterThan(0)
            .WithMessage("انتخاب دلیل تعدیل الزامی است");

        RuleFor(x => x.ItemDetailStoreId)
            .GreaterThan(0)
            .WithMessage("انتخاب جزئیات کالا الزامی است");
    }
}