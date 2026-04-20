namespace GoodHamburger.Domain.Common;

/// <summary>
/// Representa o resultado de uma operação que pode falhar com um erro de negócio esperado,
/// evitando o uso de exceções para fluxo de controle.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(true, null) => _value = value;
    private Result(string error) : base(false, error) { }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Não é possível acessar Value em um Result com falha.");

    public static Result<T> Ok(T value) => new(value);
    public static new Result<T> Failure(string error) => new(error);

    /// <summary>Desconstrução para uso com pattern matching: var (ok, value, error) = result;</summary>
    public void Deconstruct(out bool isSuccess, out T? value, out string? error)
    {
        isSuccess = IsSuccess;
        value = IsSuccess ? _value : default;
        error = Error;
    }
}
