using System;
using System.Collections.Generic;
using System.Text;
using User.Domain.Enums;

namespace User.Domain.Constants.BusinessDomain;

public static class UserConstants
{
    public static readonly HashSet<EUserStatus> BlockedUserStatuses =
    [
        EUserStatus.BANNED,
        EUserStatus.TEMPORARILY_BLOCKED
    ];

    public static readonly HashSet<EUserStatus> ByRevisionUserStatuses =
    [
        EUserStatus.REVIEW_PENDING,
        EUserStatus.ACTIVATION_PENDING
    ];
}
