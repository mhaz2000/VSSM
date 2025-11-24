using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.RecipientMaxItem;

public class RecipientMaxItemValidator : AbstractValidator<RecipientMaxItemDto>
{
    public RecipientMaxItemValidator()
    {
        RuleFor(x => x.NumberDose)
            .GreaterThanOrEqualTo(0).WithMessage("تعداد دوز باید بزرگتر یا مساوی صفر باشد")
            .When(x => x.NumberDose.HasValue);

        RuleFor(x => x.ItemId)
            .GreaterThan(0).WithMessage("شناسه آیتم باید بزرگتر از صفر باشد")
            .When(x => x.ItemId.HasValue);

        RuleFor(x => x.RecipientId)
            .GreaterThan(0).WithMessage("شناسه گیرنده باید بزرگتر از صفر باشد")
            .When(x => x.RecipientId.HasValue);

        RuleFor(x => x.ReceiverId)
            .GreaterThan(0).WithMessage("شناسه دریافت‌کننده باید بزرگتر از صفر باشد")
            .When(x => x.ReceiverId.HasValue);
    }
}
