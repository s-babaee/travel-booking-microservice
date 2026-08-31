namespace BuildingBlocks.Authorization;

public static class PermissionCatalog
{
    public const string ClaimType = "permission";
    public const string PolicyPrefix = "permission:";

    public const string BookingsCreate = "bookings.create";
    public const string BookingsReadOwn = "bookings.read.own";
    public const string BookingsCancelOwn = "bookings.cancel.own";
    public const string BookingsReadAll = "bookings.read.all";
    public const string BookingsCancelAny = "bookings.cancel.any";

    public const string HotelsView = "hotels.view";
    public const string HotelsCreate = "hotels.create";
    public const string HotelsUpdate = "hotels.update";
    public const string HotelsDelete = "hotels.delete";
    public const string HotelsInventoryManage = "hotels.inventory.manage";
    public const string FlightsView = "flights.view";
    public const string FlightsCreate = "flights.create";
    public const string FlightsUpdate = "flights.update";
    public const string FlightsDelete = "flights.delete";

    public const string PaymentsInitiate = "payments.initiate";
    public const string PaymentsViewOwn = "payments.view.own";
    public const string PaymentsViewAll = "payments.view.all";
    public const string PaymentsRefund = "payments.refund";
    public const string PaymentsVoid = "payments.void";

    public const string ReviewsRead = "reviews.read";
    public const string ReviewsCreate = "reviews.create";
    public const string ReviewsModerate = "reviews.moderate";

    public const string UsersReadAll = "users.read.all";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string ProfileReadOwn = "profile.read.own";
    public const string ProfileUpdateOwn = "profile.update.own";
    public const string NotificationsReadOwn = "notifications.read.own";
    public const string NotificationsManage = "notifications.manage";
    public const string SearchRead = "search.read";

    public static readonly IReadOnlyList<string> All =
    [
        BookingsCreate, BookingsReadOwn, BookingsCancelOwn, BookingsReadAll,
        BookingsCancelAny, HotelsView, HotelsCreate, HotelsUpdate, HotelsDelete,
        HotelsInventoryManage, FlightsView, FlightsCreate, FlightsUpdate,
        FlightsDelete, PaymentsInitiate, PaymentsViewOwn, PaymentsViewAll,
        PaymentsRefund, PaymentsVoid, ReviewsRead, ReviewsCreate, ReviewsModerate,
        UsersReadAll, UsersManage, RolesManage, ProfileReadOwn, ProfileUpdateOwn,
        NotificationsReadOwn, NotificationsManage, SearchRead
    ];

    public static string Normalize(string value) =>
        value.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase)
            ? value[PolicyPrefix.Length..].Trim().ToLowerInvariant()
            : value.Trim().ToLowerInvariant();

    public static string ToKeycloakRole(string permission) =>
        $"{PolicyPrefix}{Normalize(permission)}";
}

public static class RoleCatalog
{
    public const string Customer = "Customer";
    public const string Support = "Support";
    public const string HotelOwner = "HotelOwner";
    public const string Admin = "admin";
}
