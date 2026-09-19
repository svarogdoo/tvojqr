using System.Globalization;
using HostingQr.Application.Abstractions;
using HostingQr.Domain.Projects;

namespace HostingQr.Application.Menus;

public sealed class DigitalMenuService : IDigitalMenuService
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectLanguageVariantRepository _languageRepository;
    private readonly IDigitalMenuRepository _menuRepository;
    private readonly IEntitlementService _entitlementService;
    private readonly TimeProvider _timeProvider;
    private readonly IProjectAccessService _projectAccessService;

    public DigitalMenuService(
        ICurrentUserContext currentUserContext,
        IProjectRepository projectRepository,
        IProjectLanguageVariantRepository languageRepository,
        IDigitalMenuRepository menuRepository,
        IEntitlementService entitlementService,
        TimeProvider timeProvider,
        IProjectAccessService projectAccessService)
    {
        _currentUserContext = currentUserContext;
        _projectRepository = projectRepository;
        _languageRepository = languageRepository;
        _menuRepository = menuRepository;
        _entitlementService = entitlementService;
        _timeProvider = timeProvider;
        _projectAccessService = projectAccessService;
    }

    public async Task<DigitalMenuResponse?> GetForOwnerAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        ProjectWithSlug? project = await GetOwnedDigitalProjectAsync(projectId, cancellationToken);
        return project is null ? null : await _menuRepository.GetAsync(projectId, cancellationToken);
    }

    public async Task<DigitalMenuResponse?> SaveAsync(Guid projectId, SaveDigitalMenuRequest request, CancellationToken cancellationToken = default)
    {
        ProjectWithSlug? project = await GetOwnedDigitalProjectAsync(projectId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        ValidateTimeZone(request.TimeZone);
        ValidateCurrency(request.CurrencyCode);
        IReadOnlySet<string> languageCodes = (await _languageRepository.ListByProjectAsync(projectId, cancellationToken))
            .Select(language => language.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        ValidateMenu(request, languageCodes);

        await _menuRepository.SaveAsync(projectId, request, cancellationToken);
        return await _menuRepository.GetAsync(projectId, cancellationToken);
    }

    public async Task<bool> UpdateItemAvailabilityAsync(Guid projectId, Guid itemId, bool isOutOfStock, CancellationToken cancellationToken = default)
    {
        ProjectWithSlug? project = await GetOwnedDigitalProjectAsync(projectId, cancellationToken);
        return project is not null && await _menuRepository.UpdateItemAvailabilityAsync(projectId, itemId, isOutOfStock, cancellationToken);
    }

    public async Task<DigitalMenuResponse> GetPublicAsync(Guid projectId, string timeZone, CancellationToken cancellationToken = default)
    {
        DigitalMenuResponse menu = await _menuRepository.GetAsync(projectId, cancellationToken);
        DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, ValidateTimeZone(menu.TimeZone));
        return menu with
        {
            Categories = menu.Categories
                .Where(category => IsVisibleNow(category.Schedules, localNow))
                .Select(category => category with { Items = category.Items.Where(item => !item.IsOutOfStock).ToArray() })
                .Where(category => category.Items.Count > 0)
                .ToArray(),
        };
    }

    private async Task<ProjectWithSlug?> GetOwnedDigitalProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        Billing.EntitlementResponse entitlement = await _entitlementService.GetCurrentEntitlementAsync(cancellationToken);
        if (entitlement.Tier is not Billing.BillingTier.Plus and not Billing.BillingTier.Admin and not Billing.BillingTier.Free)
        {
            throw new InvalidOperationException("Digital Menu requires the Digital Menu plan.");
        }

        ProjectWithSlug? project = await _projectAccessService.GetAccessibleProjectAsync(projectId, cancellationToken);
        if (project is not null && project.MenuType != ProjectMenuType.Digital)
        {
            throw new InvalidOperationException("This project is not a Digital Menu.");
        }

        return project;
    }

    private static void ValidateMenu(SaveDigitalMenuRequest request, IReadOnlySet<string> languageCodes)
    {
        if (request.Categories.Count > 100)
        {
            throw new ArgumentException("A menu can contain up to 100 sections.", nameof(request));
        }

        HashSet<Guid> categoryIds = [];
        HashSet<Guid> itemIds = [];
        foreach ((DigitalMenuCategoryRequest category, int categoryIndex) in request.Categories.Select((value, index) => (value, index)))
        {
            if (category.Id == Guid.Empty || !categoryIds.Add(category.Id))
            {
                throw new ArgumentException("Every menu section needs a unique ID.", nameof(request));
            }

            ValidateTranslations(category.Translations.Select(translation => (translation.LanguageCode, translation.Name)), languageCodes, "section");
            if (category.Items.Count > 250)
            {
                throw new ArgumentException($"Section {categoryIndex + 1} contains too many items.", nameof(request));
            }

            foreach (DigitalMenuSchedule schedule in category.Schedules)
            {
                if (schedule.DayOfWeek is < 0 or > 6 || !TimeOnly.TryParseExact(schedule.StartsAt, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly startsAt) || !TimeOnly.TryParseExact(schedule.EndsAt, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly endsAt) || startsAt == endsAt)
                {
                    throw new ArgumentException("Section schedules must use a weekday from 0 to 6 and different HH:mm start and end times.", nameof(request));
                }
            }

            foreach (DigitalMenuItemRequest item in category.Items)
            {
                if (item.Id == Guid.Empty || !itemIds.Add(item.Id))
                {
                    throw new ArgumentException("Every menu item needs a unique ID.", nameof(request));
                }

                if (item.PriceText.Trim().Length > 40)
                {
                    throw new ArgumentException("Item prices can contain up to 40 characters.", nameof(request));
                }

                ValidateTranslations(item.Translations.Select(translation => (translation.LanguageCode, translation.Name)), languageCodes, "item");
            }
        }
    }

    private static void ValidateTranslations(IEnumerable<(string LanguageCode, string Name)> translations, IReadOnlySet<string> languageCodes, string contentType)
    {
        var rows = translations.ToArray();
        if (rows.Length == 0 || rows.All(row => string.IsNullOrWhiteSpace(row.Name)))
        {
            throw new ArgumentException($"Every {contentType} needs a name in at least one language.");
        }

        if (rows.Any(row => !languageCodes.Contains(row.LanguageCode)) || rows.Select(row => row.LanguageCode).Distinct(StringComparer.OrdinalIgnoreCase).Count() != rows.Length)
        {
            throw new ArgumentException($"Every {contentType} translation must use a language added to the project exactly once.");
        }
    }

    private static TimeZoneInfo ValidateTimeZone(string timeZone)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZone.Trim());
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException("Choose a valid restaurant timezone.", nameof(timeZone));
        }
        catch (InvalidTimeZoneException)
        {
            throw new ArgumentException("Choose a valid restaurant timezone.", nameof(timeZone));
        }
    }

    private static void ValidateCurrency(string currencyCode)
    {
        string normalized = currencyCode.Trim().ToUpperInvariant();
        if (normalized is not ("EUR" or "USD" or "GBP" or "CHF" or "CAD" or "AUD"))
        {
            throw new ArgumentException("Choose a supported menu currency.", nameof(currencyCode));
        }
    }

    private static bool IsVisibleNow(IReadOnlyList<DigitalMenuSchedule> schedules, DateTime localNow)
    {
        if (schedules.Count == 0)
        {
            return true;
        }

        TimeOnly currentTime = TimeOnly.FromDateTime(localNow);
        int currentDay = (int)localNow.DayOfWeek;
        int previousDay = (currentDay + 6) % 7;

        return schedules.Any(schedule =>
        {
            TimeOnly start = TimeOnly.ParseExact(schedule.StartsAt, "HH:mm", CultureInfo.InvariantCulture);
            TimeOnly end = TimeOnly.ParseExact(schedule.EndsAt, "HH:mm", CultureInfo.InvariantCulture);
            return start <= end
                ? schedule.DayOfWeek == currentDay && currentTime >= start && currentTime < end
                : (schedule.DayOfWeek == currentDay && currentTime >= start) || (schedule.DayOfWeek == previousDay && currentTime < end);
        });
    }
}
