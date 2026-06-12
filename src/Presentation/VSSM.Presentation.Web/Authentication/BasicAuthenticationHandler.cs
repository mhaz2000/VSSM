using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Presentation.Web.Authentication
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "Basic";
        public string Scheme => DefaultScheme;
        public string AuthenticationScheme => DefaultScheme;
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly VSSMDbContext _db;

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            VSSMDbContext db)
            : base(options, logger, encoder, clock)
        {
            _db = db;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Context.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Missing or invalid Authorization header");

            try
            {
                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var credentials = decodedCredentials.Split(':', 2);

                if (credentials.Length != 2)
                    return AuthenticateResult.Fail("Invalid credentials format");

                var username = credentials[0];
                var password = credentials[1];

                var user = await _db.TblUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserName == username && u.Password == password && u.IsDeleted != true);

                if (user == null)
                    return AuthenticateResult.Fail("Invalid username or password");

                var claims = new List<Claim>
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("UserName", user.UserName),
                    new Claim("FullName", user.FirstName + " " + user.LastName),
                    new Claim("RoleId", user.RoleId?.ToString() ?? "0"),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName)
                };

                if (user.RecipientId.HasValue)
                    claims.Add(new Claim("RecipientId", user.RecipientId.Value.ToString()));

                var identity = new ClaimsIdentity(claims, Options.Scheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Options.Scheme);

                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization header format");
            }
        }
    }
}