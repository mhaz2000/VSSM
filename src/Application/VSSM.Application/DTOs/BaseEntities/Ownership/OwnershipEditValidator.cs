using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Ownership
{

    public class OwnershipEditValidator : AbstractValidator<OwnershipEditDto>
    {
        public OwnershipEditValidator()
        {
            RuleFor(x => x.Code)
               .NotEmpty().WithMessage("کد اجباری است");


            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام اجباری است");

        }
    }

}
