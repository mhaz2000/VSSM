using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.FiscalPeriod
{
    public class FiscalPeriodDtoValidator : AbstractValidator<FiscalPeriodDto>
    {
        public FiscalPeriodDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام کامل دوره مالی اجباری است")
                .MaximumLength(200).WithMessage("نام کامل نمی‌تواند بیشتر از 200 کاراکتر باشد");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("تاریخ شروع اجباری است");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد")
                .When(x => x.EndTime.HasValue && x.StartDate.HasValue);

            //RuleFor(x => x.SuggestedEndDate)
            //    .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان پیشنهادی باید بعد از تاریخ شروع باشد")
            //    .When(x => x.SuggestedEndDate && x.StartDate.HasValue);
        }
    }
}
