namespace Auth.Api.Application.Exceptions;

public abstract class ApplicationExceptionBase : Exception
{
    protected ApplicationExceptionBase(string message)
        : base(message)
    {
    }
}

public sealed class NotFoundException : ApplicationExceptionBase
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}

public sealed class ConflictException : ApplicationExceptionBase
{
    public ConflictException(string message)
        : base(message)
    {
    }
}

public sealed class ValidationException : ApplicationExceptionBase
{
    public ValidationException(string message)
        : base(message)
    {
    }
}

public sealed class UnauthorizedException : ApplicationExceptionBase
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }
}

public sealed class ExternalServiceException : ApplicationExceptionBase
{
    public ExternalServiceException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
