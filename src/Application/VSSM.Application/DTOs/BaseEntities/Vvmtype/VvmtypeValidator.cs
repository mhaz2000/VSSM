using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.Vvmtype;

public class VvmtypeValidator : AbstractValidator<VvmtypeDto>
{
    public VvmtypeValidator()
    {
        // Validation for Code
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("لطفا کد را وارد کنید")
            .MaximumLength(50).WithMessage("کد نباید بیشتر از 50 کاراکتر باشد");

        // Validation for FullName
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("لطفا نام کامل را وارد کنید")
            .MaximumLength(100).WithMessage("نام کامل نباید بیشتر از 100 کاراکتر باشد");
    }
}