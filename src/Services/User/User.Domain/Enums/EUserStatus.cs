namespace User.Domain.Enums;

public enum EUserStatus
{
    ACTIVE = 1,
    INACTIVE,
    REVIEW_PENDING,
    ACTIVATION_PENDING,
    TEMPORARILY_BLOCKED,
    BANNED
}
