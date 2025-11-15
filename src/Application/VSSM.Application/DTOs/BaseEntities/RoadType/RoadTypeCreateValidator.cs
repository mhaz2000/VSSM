using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.ItemClass;

namespace VSSM.Application.DTOs.BaseEntities.RoadType
{
    public class RoadTypeCreateValidator : AbstractValidator<RoadTypeCreateDto>
    {
        public RoadTypeCreateValidator()
        {
            RuleFor(x => x.Code)
               .NotEmpty().WithMessage("کد نوع کالا اجباری است");


            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام کامل نوع کالا اجباری است");

        }
    }
}
