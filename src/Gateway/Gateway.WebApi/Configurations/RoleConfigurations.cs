using Gateway.WebApi.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Gateway.WebApi.Configurations;

internal static class RoleConfigurations
{
    internal static AuthorizationBuilder ConfigureRoles(this AuthorizationBuilder builder)
    {
        return builder
            .AddPolicy(AuthorizationConstants.Policies.RequireBidderRoleCheck, policy =>
                policy.RequireRole(AuthorizationConstants.Roles.Bidder))
            .AddPolicy(AuthorizationConstants.Policies.RequireSellerRoleCheck, policy =>
                policy.RequireRole(AuthorizationConstants.Roles.Seller))
            .AddPolicy(AuthorizationConstants.Policies.RequireAdminRoleCheck, policy =>
                policy.RequireRole(AuthorizationConstants.Roles.Admin))
            .AddPolicy(AuthorizationConstants.Policies.RequireLoggedInUserCheck, policy =>
                policy.RequireAuthenticatedUser());
    }
}
