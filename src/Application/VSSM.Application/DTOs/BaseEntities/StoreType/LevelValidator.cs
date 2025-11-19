using FluentValidation;

namespace VSSM.Application.DTOs.BaseEntities.StoreType;

public class StoreTypeValidator : AbstractValidator<StoreTypeDto>
{
    public StoreTypeValidator()
    {
        // Validation for Code (اجباری و حداکثر طول)
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("لطفا کد را وارد کنید")
            .MaximumLength(50).WithMessage("کد نباید بیشتر از 50 کاراکتر باشد");

        // Validation for FullName (اجباری و حداکثر طول)
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("لطفا نام کامل را وارد کنید")
            .MaximumLength(100).WithMessage("نام کامل نباید بیشتر از 100 کاراکتر باشد");

        // No specific rules needed for StoreTypeId, EntryDate, IsActive, and IsUserEditable
        // as they are either system-generated or optional booleans/dates.
    }
}