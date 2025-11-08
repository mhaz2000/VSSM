using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.ItemClass
{
    public class ItemClassCreateValidator : AbstractValidator<ItemClassCreateDto>
    {
        public ItemClassCreateValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد نوع کالا اجباری است")
                .MaximumLength(50);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام کامل نوع کالا اجباری است")
                .MaximumLength(200);
        }
    }
}
