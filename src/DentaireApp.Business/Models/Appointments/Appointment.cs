namespace DentaireApp.Business.Models.Appointments;

public enum AppointmentStatus
{
    Waiting = 0,
    InProgress = 1,
    Done = 2,
    Skipped = 3,
    Cancelled = 4,
}

public sealed class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public int QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Waiting;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

