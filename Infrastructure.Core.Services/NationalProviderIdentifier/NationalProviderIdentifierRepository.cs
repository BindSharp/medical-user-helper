using System.Data;
using BindSharp;
using Dapper;
using Infrastructure.Core.DTOs.NationalProviderIdentifier;
using Infrastructure.Core.Interfaces.NationalProviderIdentifier;
using Infrastructure.Core.Models.NationalProviderIdentifier;

namespace Infrastructure.Core.Services.NationalProviderIdentifier;

public sealed class NationalProviderIdentifierRepository : BaseDatabaseService, INationalProviderIdentifierRepository
{
    private readonly IDbConnection _connection;

    public NationalProviderIdentifierRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public Task<Result<Unit, NationalProviderIdentifierError>> AddAsync(NationalProviderIdentifierNumber npiNumber) =>
        ResultExtensions.TryAsync(
                operation: async () => await ExecuteInsertAsync(npiNumber),
                errorFactory: NationalProviderIdentifierError (ex) => new NationalProviderIdentifierInsertError(ex.Message, ex)
            )
            .BindAsync(affectedRows => ValidateAffectedRows(
                affectedRows, NationalProviderIdentifierError (msg) => new NationalProviderIdentifierInsertError(msg),
                "Error inserting the NPI number."
            ));

    private async Task<int> ExecuteInsertAsync(NationalProviderIdentifierNumber npiNumber)
    {
        return await _connection.ExecuteAsync(NationalProviderIdentifierSql.Insert, new
        {
            npiNumber.NationalProviderIdentifierNumberId,
            npiNumber.NationalProviderIdentifier,
            npiNumber.CreatedAt
        });
    }
}