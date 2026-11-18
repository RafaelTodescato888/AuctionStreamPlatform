namespace Gateway.WebApi.Constants;

public static class AuthorizationConstants
{
    public static class Policies
    {
        public const string RequireBidderRoleCheck = "RequireBidderRoleCheck";
        public const string RequireSellerRoleCheck = "RequireSellerRoleCheck";
        public const string RequireAdminRoleCheck = "RequireAdminRoleCheck";
        public const string RequireLoggedInUserCheck = "RequireLoggedInUserCheck";
    }

    public static class Roles
    {
        public const string Bidder = "Bidder";
        public const string Seller = "Seller";
        public const string Admin = "Admin";
    }
}
