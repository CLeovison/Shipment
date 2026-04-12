using System.Diagnostics.CodeAnalysis;
using Shipment.Abstract.Results.Errors;
using Shipment.Features.Auth.Register;

namespace Shipment.Abstract.Results;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);


    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
    public static Result<T> Create<T>(T? value) => value is not null ? Success(value) : Failure<T>(Error.NullValue);
    public override string ToString() =>
       IsSuccess ? "Success" : $"Failure: {Error.Code} - {Error.Description}";

}

public class Result<T> : Result
{
    private readonly T? _value;
    public Result(T? value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;

    [NotNull]
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Result has no value");

    public static implicit operator Result<T>(T? value) => Create(value);
    public override string ToString() => IsSuccess ? $"Success: {Value}" : $"Failure: {Error.Code} - {Error.Description}";


}