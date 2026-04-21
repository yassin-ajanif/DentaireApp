using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Models.Patients;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Repositories;

public sealed class PatientRepository(AppDbContext dbContext) : IPatientRepository
{
    public Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        dbContext.Patients
            .Include(x => x.TreatmentSheets)
            .ThenInclude(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == patientId, cancellationToken);

    public Task<Patient?> GetByNomAndTelephoneAsync(string nom, string telephone, CancellationToken cancellationToken = default) =>
        dbContext.Patients.FirstOrDefaultAsync(
            x => x.Nom == nom && x.Telephone == telephone,
            cancellationToken);

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await dbContext.Patients.AddAsync(patient, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

