using VSSM.Application.Security;

namespace VSSM.Application.Interfaces.BaseEntities.Dispatch;

public interface IDispatchAccessPolicy
{
    Task<bool> CanAsync(
        AuthorizationSubject subject,
        int? senderId,
        int? receiverId,
        bool? isConfirmed,
        ResourceOperation operation,
        CancellationToken cancellationToken = default);
}