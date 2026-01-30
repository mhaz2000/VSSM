using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Domain.Enums
{
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
}
