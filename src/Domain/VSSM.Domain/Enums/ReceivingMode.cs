using System.ComponentModel;

namespace VSSM.Domain.Enums
{
    public enum ReceivingMode
    {
        [Description("دریافت")]
        Receive,
        [Description("ارسال")]
        Send,
        [Description("دریافت و ارسال")]
        ReceiveAndSend,
    }
}
