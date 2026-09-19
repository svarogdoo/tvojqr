using Dapper;
using HostingQr.Application.Abstractions;
using HostingQr.Application.Invoices;
using HostingQr.Infrastructure.Data;

namespace HostingQr.Infrastructure.Invoices;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private const string Columns = "id, client_user_id as ClientUserId, uploaded_by_user_id as UploadedByUserId, invoice_date as InvoiceDate, original_file_name as OriginalFileName, content_type as ContentType, storage_key as StorageKey, size_bytes as SizeBytes, created_at as CreatedAt, updated_at as UpdatedAt";
    private readonly IDbConnectionFactory _connectionFactory;
    public InvoiceRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<bool> ClientExistsAsync(Guid clientUserId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            "select exists(select 1 from users where id = @ClientUserId);",
            new { ClientUserId = clientUserId }, cancellationToken: cancellationToken));
    }

    public async Task<InvoiceRecord> CreateAsync(InvoiceRecord invoice, CancellationToken cancellationToken = default)
    {
        string sql = $"insert into invoices (id, client_user_id, uploaded_by_user_id, invoice_date, original_file_name, content_type, storage_key, size_bytes) values (@Id, @ClientUserId, @UploadedByUserId, @InvoiceDate, @OriginalFileName, @ContentType, @StorageKey, @SizeBytes) returning {Columns};";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<InvoiceRecord>(new CommandDefinition(sql, invoice, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<InvoiceRecord>> ListByClientAsync(Guid clientUserId, CancellationToken cancellationToken = default)
    {
        string sql = $"select {Columns} from invoices where client_user_id = @ClientUserId order by invoice_date desc, created_at desc;";
        using var connection = _connectionFactory.CreateConnection();
        return (await connection.QueryAsync<InvoiceRecord>(new CommandDefinition(sql, new { ClientUserId = clientUserId }, cancellationToken: cancellationToken))).ToArray();
    }

    public async Task<InvoiceRecord?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        string sql = $"select {Columns} from invoices where id = @InvoiceId;";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<InvoiceRecord>(new CommandDefinition(sql, new { InvoiceId = invoiceId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(new CommandDefinition("delete from invoices where id = @InvoiceId;", new { InvoiceId = invoiceId }, cancellationToken: cancellationToken)) > 0;
    }
}
