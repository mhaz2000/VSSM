using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.DosePerVial
{
    public class DosePerVialCreateValidator : AbstractValidator<DosePerVialCreateDto>
    {
        public DosePerVialCreateValidator()
        {
            RuleFor(x => x.NumDosePerVial)
                .NotNull().WithMessage("تعداد دوز بر ویال اجباری است");
        }
    }
}
