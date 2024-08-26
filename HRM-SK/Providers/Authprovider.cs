using AutoMapper;
using HRM_SK.Database;
using HRM_SK.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRM_SK.Providers
{
    public static class AuthorizationDecisionType
    {
        public const string Applicant = "Applicant";
        public const string Staff = "Staff";
        public const string HRMUser = "HRMUser";
    }
    public class Authprovider
    {
        private readonly DatabaseContext _dbContext;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        public Authprovider(IServiceScopeFactory serviceScopeFactory)
        {
            var scope = serviceScopeFactory.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            _httpContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        }



        public async Task<User?> GetAuthUser()
        {
            ClaimsIdentity identity = _httpContext?.User.Identity as ClaimsIdentity;

            if (identity == null) return null;

            Claim UserIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            Claim authorizationDecisionClaim = identity.FindFirst(ClaimTypes.AuthorizationDecision);

            var userId = UserIdClaim?.Value;
            var applicantAuthorizationValue = authorizationDecisionClaim?.Value;

            if (userId is not null && applicantAuthorizationValue is not null)
            {
                if (applicantAuthorizationValue != AuthorizationDecisionType.HRMUser) return null;

                var user = await _dbContext
                    .User
                    .IgnoreAutoIncludes()
                    .Include(u => u.staff)
                    .FirstOrDefaultAsync(a => a.Id == Guid.Parse(userId));
                return user;
            }
            return null;
        }
    }
}
