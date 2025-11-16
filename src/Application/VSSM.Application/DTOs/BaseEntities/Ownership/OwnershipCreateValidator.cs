using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.ItemClass;

namespace VSSM.Application.DTOs.BaseEntities.Ownership;

public class OwnershipCreateValidator : AbstractValidator<OwnershipCreateDto>
{
    public OwnershipCreateValidator()
    {
        RuleFor(x => x.Code)
           .NotEmpty().WithMessage("کد اجباری است");


        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام  اجباری است");

    }
}
