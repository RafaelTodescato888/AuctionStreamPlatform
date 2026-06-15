using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace User.Domain.Helpers
{
    public static class SessionHelper
    {
        public static Guid? GetUserId(HttpContext? httpContext)
        {
            if (httpContext?.User.Identity?.IsAuthenticated == false) return null;

            var userIdClaim = httpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId)) return null;

            return userId;
        }
    }
}
