using BobsCornApp.Application.Interfaces;
using BobsCornApp.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BobsCornApp.Infrastructure.Repositories;

public class CornPurchaseRepository : ICornPurchaseRepository
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly SemaphoreSlim _databaseLock = new(1, 1);
    private bool _databaseAndSchemaEnsured;

    public CornPurchaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
        _databaseName = connectionStringBuilder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(_databaseName))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' must include a database name.");
        }
    }

    public async Task<CornPurchase?> GetLastPurchaseAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAndSchemaAsync(cancellationToken);

        const string sql = """
            SELECT TOP (1)
                ClientId,
                PurchasedAtUtc
            FROM dbo.CornPurchases
            WHERE ClientId = @ClientId;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<CornPurchase>(
            new CommandDefinition(
                sql,
                new { ClientId = clientId.Trim() },
                cancellationToken: cancellationToken));
    }

    public async Task SavePurchaseAsync(
        CornPurchase purchase,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAndSchemaAsync(cancellationToken);

        const string sql = """
            MERGE dbo.CornPurchases WITH (HOLDLOCK) AS Target
            USING (
                SELECT
                    @ClientId AS ClientId,
                    @PurchasedAtUtc AS PurchasedAtUtc
            ) AS Source
            ON Target.ClientId = Source.ClientId
            WHEN MATCHED THEN
                UPDATE SET PurchasedAtUtc = Source.PurchasedAtUtc
            WHEN NOT MATCHED THEN
                INSERT (ClientId, PurchasedAtUtc)
                VALUES (Source.ClientId, Source.PurchasedAtUtc);
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    ClientId = purchase.ClientId.Trim(),
                    purchase.PurchasedAtUtc
                },
                cancellationToken: cancellationToken));
    }

    private async Task EnsureDatabaseAndSchemaAsync(CancellationToken cancellationToken)
    {
        if (_databaseAndSchemaEnsured)
        {
            return;
        }

        await _databaseLock.WaitAsync(cancellationToken);
        try
        {
            if (_databaseAndSchemaEnsured)
            {
                return;
            }

            await EnsureDatabaseExistsAsync(cancellationToken);
            await EnsureSchemaExistsAsync(cancellationToken);

            _databaseAndSchemaEnsured = true;
        }
        finally
        {
            _databaseLock.Release();
        }
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF DB_ID(@DatabaseName) IS NULL
            BEGIN
                DECLARE @createDatabaseStatement NVARCHAR(MAX) =
                    N'CREATE DATABASE ' + QUOTENAME(@DatabaseName);

                EXEC (@createDatabaseStatement);
            END;
            """;

        var connectionStringBuilder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        await using var connection = new SqlConnection(connectionStringBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { DatabaseName = _databaseName },
            cancellationToken: cancellationToken));
    }

    private async Task EnsureSchemaExistsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.CornPurchases', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.CornPurchases
                (
                    ClientId NVARCHAR(256) NOT NULL CONSTRAINT PK_CornPurchases PRIMARY KEY,
                    PurchasedAtUtc DATETIMEOFFSET NOT NULL
                );
            END;
            """;

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
