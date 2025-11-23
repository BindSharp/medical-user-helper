using System.Data;
using BindSharp;
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
    
    public async Task<Result<Unit, NationalProviderIdentifierError>> AddAsync(NationalProviderIdentifierNumber npiNumber) =>
        await ResultExtensions.TryAsync(
                operation: async () => await ExecuteNonQueryAsync(_connection, NationalProviderIdentifierSql.Insert, npiNumber),
                errorFactory: NationalProviderIdentifierError (ex) => new NationalProviderIdentifierInsertError(ex.Message, ex)
            )
            .BindAsync(affectedRows => ValidateAffectedRows(
                affectedRows, NationalProviderIdentifierError (msg) => new NationalProviderIdentifierInsertError(msg),
                "Error inserting the NPI number."
            ));

    public async Task<Result<IEnumerable<NationalProviderIdentifierNumber>, NationalProviderIdentifierError>> GetAllAsync() =>
        await ResultExtensions.TryAsync(
                operation: async () => await ExecuteQueryAsync<NationalProviderIdentifierNumber>(_connection, NationalProviderIdentifierSql.GetAll),
                errorFactory: NationalProviderIdentifierError (ex) => new GetAllNationalProviderIdentifiersError(ex.Message, ex)
            );
}