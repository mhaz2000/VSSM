using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.AdjustReason;

public class AdjustReasonValidator : AbstractValidator<AdjustreasonDto>
{
    public AdjustReasonValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد نمی‌تواند خالی باشد")
            .MaximumLength(50).WithMessage("کد نمی‌تواند بیش از ۵۰ کاراکتر باشد");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام کامل نمی‌تواند خالی باشد")
            .MaximumLength(200).WithMessage("نام کامل نمی‌تواند بیش از ۲۰۰ کاراکتر باشد");

        RuleFor(x => x.EntryDate)
            .LessThanOrEqualTo(DateTime.Now)
            .WithMessage("تاریخ ورود نمی‌تواند بزرگتر از امروز باشد")
            .When(x => x.EntryDate.HasValue);

        // Boolean fields (all optional)
        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("وضعیت فعال/غیرفعال باید مشخص شود");

        RuleFor(x => x.UsedForDispatchAdjust)
            .NotNull().WithMessage("وضعیت استفاده برای تعدیل ارسال باید مشخص شود");

        RuleFor(x => x.UserForArrivalAdjust)
            .NotNull().WithMessage("وضعیت استفاده برای تعدیل رسید باید مشخص شود");

        RuleFor(x => x.UsedForStockAdjust)
            .NotNull().WithMessage("وضعیت استفاده برای تعدیل موجودی باید مشخص شود");

        RuleFor(x => x.IsWaste)
            .NotNull().WithMessage("وضعیت ضایعات باید مشخص شود");

        RuleFor(x => x.ApplicationForStockSubtraction)
            .NotNull().WithMessage("وضعیت کاربرد در کاهش موجودی باید مشخص شود");

        RuleFor(x => x.ApplicationForStockAddition)
            .NotNull().WithMessage("وضعیت کاربرد در افزایش موجودی باید مشخص شود");
    }
}