using System.Data;
using BindSharp;
using Infrastructure.Core.DTOs.License;
using Infrastructure.Core.Interfaces.License;
using Infrastructure.Core.Models.License;

namespace Infrastructure.Core.Services.License;

public sealed class LicenseNumberRepository : BaseDatabaseService, ILicenseNumberRepository
{
    private readonly IDbConnection _connection;

    public LicenseNumberRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<Result<Unit, LicenseNumberError>> AddAsync(LicenseNumber licenseNumber) =>
        await ResultExtensions.TryAsync(
                operation: async () => await ExecuteInsertAsync(_connection, LicenseNumberSql.Insert, licenseNumber),
                errorFactory: LicenseNumberError (ex) => new LicenseNumberInsertError(ex.Message, ex)
            )
            .BindAsync(affectedRows => ValidateAffectedRows<LicenseNumberError>(
                affectedRows,
                msg => new LicenseNumberInsertError(msg),
                "Error inserting the License number."
            ));
}