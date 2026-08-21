namespace Inventory.Api.Application.Exceptions;

public abstract class ApplicationExceptionBase : Exception
{
    protected ApplicationExceptionBase(string message)
        : base(message)
    {
    }
}

public sealed class NotFoundException : ApplicationExceptionBase
{
    public NotFoundException(string resource, Guid id)
        : base($"{resource} with id '{id}' was not found.")
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
