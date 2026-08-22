namespace Notification.Application.Exceptions;

public abstract class ApplicationExceptionBase(string message)
    : Exception(message);

public sealed class NotFoundException(string resource, Guid id)
    : ApplicationExceptionBase($"{resource} with id '{id}' was not found.");

public sealed class ConflictException(string message)
    : ApplicationExceptionBase(message);

public sealed class ValidationException(string message)
    : ApplicationExceptionBase(message);

public sealed class UnauthorizedException(string message)
    : ApplicationExceptionBase(message);
