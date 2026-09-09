namespace HostingQr.Application.Menus;

public sealed record DigitalMenuResponse(string TimeZone, IReadOnlyList<DigitalMenuCategoryResponse> Categories);

public sealed record DigitalMenuCategoryResponse(
    Guid Id,
    int SortOrder,
    IReadOnlyList<DigitalMenuCategoryTranslation> Translations,
    IReadOnlyList<DigitalMenuSchedule> Schedules,
    IReadOnlyList<DigitalMenuItemResponse> Items);

public sealed record DigitalMenuCategoryTranslation(string LanguageCode, string Name);

public sealed record DigitalMenuSchedule(int DayOfWeek, string StartsAt, string EndsAt);

public sealed record DigitalMenuItemResponse(
    Guid Id,
    string PriceText,
    bool IsOutOfStock,
    int SortOrder,
    IReadOnlyList<DigitalMenuItemTranslation> Translations);

public sealed record DigitalMenuItemTranslation(string LanguageCode, string Name, string Description);

public sealed record SaveDigitalMenuRequest(string TimeZone, IReadOnlyList<DigitalMenuCategoryRequest> Categories);

public sealed record DigitalMenuCategoryRequest(
    Guid Id,
    IReadOnlyList<DigitalMenuCategoryTranslation> Translations,
    IReadOnlyList<DigitalMenuSchedule> Schedules,
    IReadOnlyList<DigitalMenuItemRequest> Items);

public sealed record DigitalMenuItemRequest(
    Guid Id,
    string PriceText,
    bool IsOutOfStock,
    IReadOnlyList<DigitalMenuItemTranslation> Translations);

public sealed record UpdateDigitalMenuItemAvailabilityRequest(bool IsOutOfStock);
