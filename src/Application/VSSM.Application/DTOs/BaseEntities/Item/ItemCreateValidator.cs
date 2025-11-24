using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Item
{
    public class ItemCreateValidator : AbstractValidator<ItemCreateDto>
    {
        public ItemCreateValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("کد کالا اجباری است")
                .MaximumLength(50);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("نام کامل کالا اجباری است")
                .MaximumLength(200);
        }
    }
}
