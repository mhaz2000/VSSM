using System.ComponentModel;

namespace VSSM.Domain.Enums
{
    public enum TransportMode
    {
        [Description("ماشین سردخانه")]
        ColdStorageCar,
        [Description("کلد باکس")]
        ColdBox,
        [Description("ماشین دارای یخچال")]
        CarWithRefrigerator,
    }
}
