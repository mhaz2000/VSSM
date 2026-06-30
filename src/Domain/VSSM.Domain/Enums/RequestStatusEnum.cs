using System.ComponentModel.DataAnnotations;

namespace VSSM.Domain.Enums;

public enum RequestStatusEnum
{
    [Display(Name = "برچسب")]
    Label = 0,

    [Display(Name = "پیشنویس")]
    Draft = 1,

    [Display(Name = "ارسال شده")]
    Sent = 2,

    [Display(Name = "تایید شده")]
    Confirmed = 3
}
