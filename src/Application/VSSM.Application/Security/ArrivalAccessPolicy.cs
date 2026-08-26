using System.Security.Claims;
using VSSM.Application.Interfaces;
using VSSM.Application.Security;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Infrastructure.Security;

public sealed class ArrivalAccessPolicy : IResourceAccessPolicy<TblArrival>
{
    public Task<bool> CanAsync(TblArrival arrival, ResourceOperation operation, ClaimsPrincipal user)
    {
        var isDrafted = arrival.IsDrafted == true;
        var isConfirmed = arrival.IsConfirmed == true;
        var isCancelled = arrival.IsCancelled == true;

        var allowed = operation switch
        {
            ResourceOperation.Read => isConfirmed,
            ResourceOperation.Edit => isDrafted && !isConfirmed,
            ResourceOperation.Delete => isDrafted && !isConfirmed,
            ResourceOperation.Print => isConfirmed,
            ResourceOperation.Confirm => isDrafted && !isConfirmed,
            ResourceOperation.Create => true,
            _ => false
        };

        return Task.FromResult(allowed);
    }

    public IQueryable<TblArrival> ApplyReadFilter(IQueryable<TblArrival> query, ClaimsPrincipal user)
    {
        return query;
    }
}