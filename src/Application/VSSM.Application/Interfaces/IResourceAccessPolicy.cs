using System.Security.Claims;
using VSSM.Application.Security;

namespace VSSM.Application.Interfaces;

public interface IResourceAccessPolicy<TResource> where TResource : class
{
    Task<bool> CanAsync(TResource resource, ResourceOperation operation, ClaimsPrincipal user);
    IQueryable<TResource> ApplyReadFilter(IQueryable<TResource> query, ClaimsPrincipal user);
}
