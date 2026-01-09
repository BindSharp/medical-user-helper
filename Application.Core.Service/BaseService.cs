using BindSharp;

namespace Application.Core.Service;

public abstract class BaseService
{
    protected static Result<Unit, TError> IsStringEmpty<TError>(string? lastName) 
        where TError : new() =>
        string.IsNullOrEmpty(lastName) || string.IsNullOrWhiteSpace(lastName)
            ? Result<Unit, TError>.Failure(new TError())
            : Result<Unit, TError>.Success(Unit.Value);
}