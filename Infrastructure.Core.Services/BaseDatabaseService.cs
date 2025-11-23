using System.Data;
using BindSharp;
using Dapper;

namespace Infrastructure.Core.Services;

public class BaseDatabaseService
{
    protected static Result<Unit, TError> ValidateAffectedRows<TError>(
        int affectedRows,
        Func<string, TError> errorFactory,
        string errorMessage) =>
        affectedRows > 0
            ? Result<Unit, TError>.Success(Unit.Value)
            : Result<Unit, TError>.Failure(errorFactory(errorMessage));
    
    protected static async Task<int> ExecuteInsertAsync<TIn>(IDbConnection connection, string sql, TIn entity) => 
        await connection.ExecuteAsync(sql, entity);

    protected static Task<Result<T?, TError>> ExecuteQueryAsync<T, TError>(
        IDbConnection connection,
        string sql,
        object parameters,
        Func<string, Exception, TError> errorFactory) =>
        ResultExtensions.TryAsync(
            operation: async () => await connection.QueryFirstOrDefaultAsync<T>(sql, parameters),
            errorFactory: ex => errorFactory(ex.Message, ex)
        );
}