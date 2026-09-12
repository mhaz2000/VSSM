using System.Security.Claims;
using VSSM.Application.Interfaces;
using VSSM.Application.Security;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Infrastructure.Security;

public sealed class RequestAccessPolicy : IResourceAccessPolicy<TblRequest>
{
    public Task<bool> CanAsync(
        TblRequest request,
        ResourceOperation operation,
        ClaimsPrincipal user)
    {
        var currentRecipientId = GetCurrentRecipientId(user);

        if (currentRecipientId is null)
            return Task.FromResult(false);

        var isSender = request.SenderId == currentRecipientId.Value;
        var isReceiver = request.ReceiverId == currentRecipientId.Value;

        var status = (RequestStatusEnum)request.Status;
        var isConfirmed = status == RequestStatusEnum.Confirmed;
        var isSent = status == RequestStatusEnum.Sent;

        var allowed = operation switch
        {
            ResourceOperation.Create =>
                true,

            ResourceOperation.Read =>
                isSender || isReceiver,

            ResourceOperation.Edit =>
                isSender && !isConfirmed,

            ResourceOperation.Delete =>
                isSender && !isConfirmed,

            ResourceOperation.Print =>
                isSender || isReceiver,

            ResourceOperation.Confirm =>
                isReceiver && isSent,

            ResourceOperation.Reject =>
                isReceiver && isSent,

            _ => false
        };

        return Task.FromResult(allowed);
    }

    public IQueryable<TblRequest> ApplyReadFilter(
        IQueryable<TblRequest> query,
        ClaimsPrincipal user)
    {
        var currentRecipientId = GetCurrentRecipientId(user);

        if (currentRecipientId is null)
            return query.Where(x => false);

        return query.Where(x =>
            x.SenderId == currentRecipientId.Value ||
            x.ReceiverId == currentRecipientId.Value);
    }

    private static int? GetCurrentRecipientId(ClaimsPrincipal user)
    {
        var recipientId = user.FindFirst("RecipientId")?.Value;

        return int.TryParse(recipientId, out var id)
            ? id
            : null;
    }
}