namespace FlowX.Responses;

/// <summary>
/// Represents a unified response wrapper for FlowX request/reply operations.
/// way to handle both successful responses and error scenarios.
/// </summary>
public sealed class Result<T>
{
    /// <summary>
    /// Gets whether the request was processed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the response data when the request is successful.
    /// Will be null when <see cref="IsSuccess"/> is false.
    /// </summary>
    public T Data { get; init; }

    /// <summary>
    /// Gets the fault information when the request failed.
    /// Will be null when <see cref="IsSuccess"/> is true.
    /// </summary>
    public Fault Fault { get; init; }

    /// <summary>
    /// Creates a successful response with the given data.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful T containing the data.</returns>
    public static Result<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data,
        Fault = null
    };

    /// <summary>
    /// Creates a failed response with fault information.
    /// </summary>
    /// <param name="fault">The fault information.</param>
    /// <returns>A failed T containing the fault.</returns>
    public static Result<T> Failed(Fault fault) => new()
    {
        IsSuccess = false,
        Data = default,
        Fault = fault
    };

    /// <summary>
    /// Creates a failed response from an exception.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="faultedMessageId">Optional identifier for the faulted message.</param>
    /// <returns>A failed T containing the fault information.</returns>
    public static Result<T> Failed(Exception exception, string faultedMessageId = null) => new()
    {
        IsSuccess = false,
        Data = default,
        Fault = Fault.FromException(exception, faultedMessageId)
    };
}