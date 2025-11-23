using System.Data;
using BindSharp;
using Infrastructure.Core.DTOs.DEARegistrationNumber;
using Infrastructure.Core.Interfaces.DEARegistrationNumber;
using Infrastructure.Core.Models.DEARegistrationNumber;

namespace Infrastructure.Core.Services.DEARegistrationNumber;

public sealed class NdeaRegistrationNumberRepository : BaseDatabaseService, INdeaRegistrationNumberRepository
{
    private readonly IDbConnection _connection;

    public NdeaRegistrationNumberRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<Result<Unit, NdeaRegistrationNumberError>> AddAsync(NarcoticDrugEnforcementAddictionNumber ndeaNumber) =>
        await ResultExtensions.TryAsync(
                operation: async () => await ExecuteNonQueryAsync(_connection, NdeaRegistrationNumberSql.Insert, new
                {
                    NdeaRegistrationNumberId = ndeaNumber.NarcoticDrugEnforcementAddictionNumberId,
                    NdeaRegistrationNumberValue = ndeaNumber.NarcoticDrugEnforcementAddictionNumberValue,
                    CreatedAt = ndeaNumber.CreatedAt
                }),
                errorFactory: NdeaRegistrationNumberError (ex) => new NdeaRegistrationNumberInsertError(ex.Message, ex)
            )
            .BindAsync(affectedRows => ValidateAffectedRows<NdeaRegistrationNumberError>(
                affectedRows,
                msg => new NdeaRegistrationNumberInsertError(msg),
                "Error inserting the NDEA number."
            ));
}