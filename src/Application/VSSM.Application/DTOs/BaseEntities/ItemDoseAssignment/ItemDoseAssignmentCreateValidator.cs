using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;

namespace VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment
{
    public class ItemDoseAssignmentCreateValidator : AbstractValidator<ItemDoseAssignmentCreateDto>
    {
        public ItemDoseAssignmentCreateValidator()
        {
            RuleFor(x => x.ItemId)
                .NotNull().WithMessage("انتخاب کالا اجباری است");

            RuleFor(x => x.DoseId)
                .NotNull().WithMessage("انتخاب دوز بر ویال اجباری است");
        }
    }
}
