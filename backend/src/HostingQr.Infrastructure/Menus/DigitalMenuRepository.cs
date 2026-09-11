using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Menus;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Menus;

public sealed class DigitalMenuRepository : IDigitalMenuRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DigitalMenuRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DigitalMenuResponse> GetAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            select time_zone as TimeZone, time_zone_configured as TimeZoneConfigured, currency_code as CurrencyCode from projects where id = @ProjectId;
            select id, sort_order as SortOrder from menu_categories where project_id = @ProjectId order by sort_order;
            select t.category_id as CategoryId, l.language_code as LanguageCode, t.name
            from menu_category_translations t
            inner join project_language_variants l on l.id = t.language_variant_id
            inner join menu_categories c on c.id = t.category_id
            where c.project_id = @ProjectId;
            select s.category_id as CategoryId, s.day_of_week::integer as DayOfWeek,
                   to_char(s.starts_at, 'HH24:MI') as StartsAt, to_char(s.ends_at, 'HH24:MI') as EndsAt
            from menu_category_schedules s
            inner join menu_categories c on c.id = s.category_id
            where c.project_id = @ProjectId order by s.day_of_week, s.starts_at;
            select i.id, i.category_id as CategoryId, i.price_text as PriceText, i.is_out_of_stock as IsOutOfStock, i.sort_order as SortOrder
            from menu_items i
            inner join menu_categories c on c.id = i.category_id
            where c.project_id = @ProjectId order by i.sort_order;
            select t.item_id as ItemId, l.language_code as LanguageCode, t.name, t.description
            from menu_item_translations t
            inner join project_language_variants l on l.id = t.language_variant_id
            inner join menu_items i on i.id = t.item_id
            inner join menu_categories c on c.id = i.category_id
            where c.project_id = @ProjectId;
            """;

        using var connection = _connectionFactory.CreateConnection();
        using var result = await connection.QueryMultipleAsync(new CommandDefinition(sql, new { ProjectId = projectId }, cancellationToken: cancellationToken));
        SettingsRow settings = await result.ReadSingleAsync<SettingsRow>();
        CategoryRow[] categories = (await result.ReadAsync<CategoryRow>()).ToArray();
        CategoryTranslationRow[] categoryTranslations = (await result.ReadAsync<CategoryTranslationRow>()).ToArray();
        ScheduleRow[] schedules = (await result.ReadAsync<ScheduleRow>()).ToArray();
        ItemRow[] items = (await result.ReadAsync<ItemRow>()).ToArray();
        ItemTranslationRow[] itemTranslations = (await result.ReadAsync<ItemTranslationRow>()).ToArray();

        return new DigitalMenuResponse(settings.TimeZone, settings.TimeZoneConfigured, settings.CurrencyCode, categories.Select(category => new DigitalMenuCategoryResponse(
            category.Id,
            category.SortOrder,
            categoryTranslations.Where(row => row.CategoryId == category.Id).Select(row => new DigitalMenuCategoryTranslation(row.LanguageCode, row.Name)).ToArray(),
            schedules.Where(row => row.CategoryId == category.Id).Select(row => new DigitalMenuSchedule(row.DayOfWeek, row.StartsAt, row.EndsAt)).ToArray(),
            items.Where(item => item.CategoryId == category.Id).Select(item => new DigitalMenuItemResponse(
                item.Id,
                item.PriceText,
                item.IsOutOfStock,
                item.SortOrder,
                itemTranslations.Where(row => row.ItemId == item.Id).Select(row => new DigitalMenuItemTranslation(row.LanguageCode, row.Name, row.Description)).ToArray())).ToArray())).ToArray());
    }

    public async Task SaveAsync(Guid projectId, SaveDigitalMenuRequest request, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var languages = (await connection.QueryAsync<LanguageRow>(new CommandDefinition(
            "select id, language_code as LanguageCode from project_language_variants where project_id = @ProjectId;",
            new { ProjectId = projectId }, transaction, cancellationToken: cancellationToken)))
            .ToDictionary(language => language.LanguageCode, language => language.Id, StringComparer.OrdinalIgnoreCase);

        await connection.ExecuteAsync(new CommandDefinition("delete from menu_categories where project_id = @ProjectId;", new { ProjectId = projectId }, transaction, cancellationToken: cancellationToken));
        await connection.ExecuteAsync(new CommandDefinition("update projects set time_zone = @TimeZone, time_zone_configured = true, currency_code = @CurrencyCode, updated_at = now() where id = @ProjectId;", new { ProjectId = projectId, request.TimeZone, CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant() }, transaction, cancellationToken: cancellationToken));

        for (int categoryIndex = 0; categoryIndex < request.Categories.Count; categoryIndex++)
        {
            DigitalMenuCategoryRequest category = request.Categories[categoryIndex];
            await connection.ExecuteAsync(new CommandDefinition(
                "insert into menu_categories (id, project_id, sort_order) values (@Id, @ProjectId, @SortOrder);",
                new { category.Id, ProjectId = projectId, SortOrder = categoryIndex }, transaction, cancellationToken: cancellationToken));

            foreach (DigitalMenuCategoryTranslation translation in category.Translations)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "insert into menu_category_translations (category_id, language_variant_id, name) values (@CategoryId, @LanguageId, @Name);",
                    new { CategoryId = category.Id, LanguageId = languages[translation.LanguageCode], Name = translation.Name.Trim() }, transaction, cancellationToken: cancellationToken));
            }

            foreach (DigitalMenuSchedule schedule in category.Schedules)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    "insert into menu_category_schedules (id, category_id, day_of_week, starts_at, ends_at) values (@Id, @CategoryId, @DayOfWeek, cast(@StartsAt as time), cast(@EndsAt as time));",
                    new { Id = Guid.NewGuid(), CategoryId = category.Id, schedule.DayOfWeek, schedule.StartsAt, schedule.EndsAt }, transaction, cancellationToken: cancellationToken));
            }

            for (int itemIndex = 0; itemIndex < category.Items.Count; itemIndex++)
            {
                DigitalMenuItemRequest item = category.Items[itemIndex];
                await connection.ExecuteAsync(new CommandDefinition(
                    "insert into menu_items (id, category_id, price_text, is_out_of_stock, sort_order) values (@Id, @CategoryId, @PriceText, @IsOutOfStock, @SortOrder);",
                    new { item.Id, CategoryId = category.Id, PriceText = item.PriceText.Trim(), item.IsOutOfStock, SortOrder = itemIndex }, transaction, cancellationToken: cancellationToken));

                foreach (DigitalMenuItemTranslation translation in item.Translations)
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        "insert into menu_item_translations (item_id, language_variant_id, name, description) values (@ItemId, @LanguageId, @Name, @Description);",
                        new { ItemId = item.Id, LanguageId = languages[translation.LanguageCode], Name = translation.Name.Trim(), Description = translation.Description.Trim() }, transaction, cancellationToken: cancellationToken));
                }
            }
        }

        transaction.Commit();
    }

    public async Task<bool> UpdateItemAvailabilityAsync(Guid projectId, Guid itemId, bool isOutOfStock, CancellationToken cancellationToken = default)
    {
        const string sql = """
            update menu_items i
            set is_out_of_stock = @IsOutOfStock
            from menu_categories c
            where i.id = @ItemId and i.category_id = c.id and c.project_id = @ProjectId;
            """;
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(new CommandDefinition(sql, new { ProjectId = projectId, ItemId = itemId, IsOutOfStock = isOutOfStock }, cancellationToken: cancellationToken)) > 0;
    }

    private sealed record CategoryRow(Guid Id, int SortOrder);
    private sealed record SettingsRow(string TimeZone, bool TimeZoneConfigured, string CurrencyCode);
    private sealed record CategoryTranslationRow(Guid CategoryId, string LanguageCode, string Name);
    private sealed record ScheduleRow(Guid CategoryId, int DayOfWeek, string StartsAt, string EndsAt);
    private sealed record ItemRow(Guid Id, Guid CategoryId, string PriceText, bool IsOutOfStock, int SortOrder);
    private sealed record ItemTranslationRow(Guid ItemId, string LanguageCode, string Name, string Description);
    private sealed record LanguageRow(Guid Id, string LanguageCode);
}
