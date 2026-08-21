using Inventory.Api.Application.Exceptions;

namespace Inventory.Api.Application.Services;

internal static class InventoryServiceHelpers
{
    public static DateTime Utc(DateTime? value, DateTime nowUtc)
    {
        var result = value ?? nowUtc.AddMinutes(10);
        return result.Kind switch
        {
            DateTimeKind.Utc => result,
            DateTimeKind.Local => result.ToUniversalTime(),
            _ => DateTime.SpecifyKind(result, DateTimeKind.Utc)
        };
    }

    public static IReadOnlyList<DateOnly> Dates(DateOnly from, DateOnly to)
    {
        if (to <= from)
        {
            throw new ValidationException(
                "The end date must be after the start date.");
        }

        var days = to.DayNumber - from.DayNumber;
        if (days > 366)
        {
            throw new ValidationException(
                "An inventory request cannot span more than 366 days.");
        }

        return Enumerable.Range(0, days)
            .Select(offset => from.AddDays(offset))
            .ToArray();
    }

    public static void ValidateId(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException($"{name} is required.");
        }
    }
}
