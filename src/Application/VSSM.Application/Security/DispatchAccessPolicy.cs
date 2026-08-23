using VSSM.Application.Interfaces.BaseEntities.Dispatch;
using VSSM.Application.Security;

namespace VSSM.Infrastructure.Security;

public sealed class DispatchAccessPolicy : IDispatchAccessPolicy
{
    public Task<bool> CanAsync(
        AuthorizationSubject subject,
        int? senderId,
        int? receiverId,
        bool? isConfirmed,
        ResourceOperation operation,
        CancellationToken cancellationToken = default)
    {
        if (subject.RecipientId is null)
            return Task.FromResult(false);

        var currentRecipientId = subject.RecipientId.Value;

        var isSender = senderId == currentRecipientId;
        var isReceiver = receiverId == currentRecipientId;
        var isConfirmedDispatch = isConfirmed == true;

        var allowed = operation switch
        {
            // مشاهده فقط رکوردهای تاییدشده‌ای که
            // متعلق به فرستنده فعلی هستند
            ResourceOperation.Read =>
                isSender && isConfirmedDispatch,

            // ویرایش فقط پیش‌فاکتورهای خود کاربر
            // که هنوز تایید نشده‌اند
            ResourceOperation.Edit =>
                isSender && !isConfirmedDispatch,

            // حذف فقط رکوردهای خود کاربر
            // که هنوز تایید نشده‌اند
            ResourceOperation.Delete =>
                isSender && !isConfirmedDispatch,

            // چاپ برای فرستنده یا گیرنده
            ResourceOperation.Print =>
                (isSender || isReceiver) && isConfirmedDispatch,

            ResourceOperation.Confirm =>
                isSender && !isConfirmedDispatch,

            ResourceOperation.Receive =>
                isReceiver && !isConfirmedDispatch,

            _ => false
        };

        return Task.FromResult(allowed);
    }
}