using System.Security.Claims;
using VSSM.Application.Interfaces;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Security;

public sealed class DispatchAccessPolicy : IResourceAccessPolicy<TblDispatch>
{
    public Task<bool> CanAsync(
        TblDispatch dispatch,
        ResourceOperation operation,
        ClaimsPrincipal user)
    {
        var currentRecipientId = GetCurrentRecipientId(user);

        if (currentRecipientId is null)
            return Task.FromResult(false);

        var isSender = dispatch.SenderId == currentRecipientId.Value;
        var isReceiver = dispatch.ReceiverId == currentRecipientId.Value;
        var isConfirmed = dispatch.IsConfirmed == true;

        var allowed = operation switch
        {
            ResourceOperation.Read =>
                isSender,

            ResourceOperation.Edit =>
                isSender && !isConfirmed,

            ResourceOperation.Delete =>
                isSender && !isConfirmed,

            ResourceOperation.Print =>
                isSender,

            ResourceOperation.Confirm =>
                isSender && !isConfirmed,

            ResourceOperation.Receive =>
                isReceiver &&
                isConfirmed &&
                dispatch.ReceiverStatus == 0,

            _ => false
        };

        return Task.FromResult(allowed);
    }

    public IQueryable<TblDispatch> ApplyReadFilter(
        IQueryable<TblDispatch> query,
        ClaimsPrincipal user)
    {
        var currentRecipientId = GetCurrentRecipientId(user);

        if (currentRecipientId is null)
            return query.Where(x => false);

        return query.Where(x =>
            x.SenderId == currentRecipientId.Value);
    }

    private static int? GetCurrentRecipientId(ClaimsPrincipal user)
    {
        var recipientId = user.FindFirst("RecipientId")?.Value;

        return int.TryParse(recipientId, out var id)
            ? id
            : null;
    }
}