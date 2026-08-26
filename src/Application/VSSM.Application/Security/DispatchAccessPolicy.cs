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
            ResourceOperation.Read =>
                isSender && isConfirmedDispatch,

            ResourceOperation.Edit =>
                isSender && !isConfirmedDispatch,

            ResourceOperation.Delete =>
                isSender && !isConfirmedDispatch,

            ResourceOperation.Print =>
                isSender && isConfirmedDispatch,

            ResourceOperation.Confirm =>
                isSender && !isConfirmedDispatch,

            ResourceOperation.Receive =>
                isReceiver && !isConfirmedDispatch,

            _ => false
        };

        return Task.FromResult(allowed);
    }
}