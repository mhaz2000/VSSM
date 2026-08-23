namespace VSSM.Application.Security;

public sealed record AuthorizationSubject(
    int UserId,
    int? RecipientId,
    IReadOnlyCollection<string> Roles);