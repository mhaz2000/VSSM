using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;

namespace VSSM.Application.DTOs.BaseEntities.ItemClass
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
