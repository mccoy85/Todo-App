namespace TodoApi.Api.Validators;

// Shared validation helpers for API-level rules.
public static class ValidationHelpers
{
    public static bool BeTodayOrLater(DateTime? dueDate)
    {
        if (!dueDate.HasValue)
        {
            return true;
        }

        // Allow a 12-hour buffer so "today" remains valid across time zones.
        var utcFloor = DateTime.UtcNow.AddHours(-12).Date;
        return dueDate.Value.Date >= utcFloor;
    }
}
