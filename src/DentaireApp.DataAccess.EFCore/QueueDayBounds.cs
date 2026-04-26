using System;

namespace DentaireApp.DataAccess.EFCore;

/// <summary>Local calendar day as UTC bounds for querying <see cref="Models.Appointments.Appointment.StartedAt"/> (stored as UTC unix).</summary>
internal static class QueueDayBounds
{
    public static (DateTime UtcStart, DateTime UtcEnd) ForLocalCalendarDate(DateOnly date)
    {
        var dayStartLocal = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local);
        var dayEndLocal = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Local);
        return (dayStartLocal.ToUniversalTime(), dayEndLocal.ToUniversalTime());
    }
}
