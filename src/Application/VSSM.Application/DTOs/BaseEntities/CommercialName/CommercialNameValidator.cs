using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.CommercialName;

public class CommercialNameValidator : AbstractValidator<CommercialNameDto>
{
    public CommercialNameValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام کامل الزامی است")
            .MaximumLength(200).WithMessage("نام کامل نمی‌تواند بیش از 200 کاراکتر باشد");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد الزامی است")
            .MaximumLength(50).WithMessage("کد نمی‌تواند بیش از 50 کاراکتر باشد");

        RuleFor(x => x.EntryDate)
            .LessThanOrEqualTo(DateTime.Now).WithMessage("تاریخ ورود نمی‌تواند در آینده باشد")
            .When(x => x.EntryDate.HasValue);
    }
}