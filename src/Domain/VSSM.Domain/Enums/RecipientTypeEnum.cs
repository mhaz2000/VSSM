using System.ComponentModel.DataAnnotations;

namespace VSSM.Domain.Enums
{
    public enum RecipientTypeEnum
    {
        [Display(Name = "انبار")]
        Store = 1,

        [Display(Name = "مرکز واکسیناسیون")]
        VaccinationCenter = 2,

        [Display(Name = "انبار و مرکز واکسیناسیون")]
        StoreAndVaccinationCenter = 3
    }
}
