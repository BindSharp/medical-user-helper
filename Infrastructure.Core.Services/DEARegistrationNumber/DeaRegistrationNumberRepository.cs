using System.Data;
using BindSharp;
using Dapper;
using Infrastructure.Core.DTOs.DEARegistrationNumber;
using Infrastructure.Core.Interfaces.DEARegistrationNumber;
using Infrastructure.Core.Models.DEARegistrationNumber;

namespace Infrastructure.Core.Services.DEARegistrationNumber;

public sealed class DeaRegistrationNumberRepository : BaseDatabaseService, IDeaRegistrationNumberRepository
{
    private readonly IDbConnection _connection;

    public DeaRegistrationNumberRepository(IDbConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<Result<Unit, DeaRegistrationNumberError>> AddAsync(DeaRegistrationNumber deaNumber) =>
        await ResultExtensions.TryAsync(
                operation: async () => await ExecuteInsertAsync(_connection, DeaRegistrationNumberSql.Insert, deaNumber),
                errorFactory: DeaRegistrationNumberError (ex) => new DeaRegistrationNumberInsertError(ex.Message, ex)
            )
            .BindAsync(affectedRows => ValidateAffectedRows<DeaRegistrationNumberError>(
                affectedRows,
                msg => new DeaRegistrationNumberInsertError(msg),
                "Error inserting the DEA number."
            ));
    
    private async Task<int> ExecuteInsertAsync(DeaRegistrationNumber deaNumber)
    {
        return await _connection.ExecuteAsync(DeaRegistrationNumberSql.Insert, new
        {
            deaNumber.DeaRegistrationNumberId,
            deaNumber.DeaRegistrationNumberValue,
            deaNumber.CreatedAt
        });
    }
}