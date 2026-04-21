using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Models.Patients;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Repositories;

public sealed class TreatmentRepository(AppDbContext dbContext) : ITreatmentRepository
{
    public async Task<IReadOnlyList<TreatmentSheet>> GetSheetsByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        await dbContext.TreatmentSheets
            .Include(x => x.Lines)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<TreatmentSheet?> GetSheetByIdAsync(Guid sheetId, CancellationToken cancellationToken = default) =>
        dbContext.TreatmentSheets.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == sheetId, cancellationToken);

    public async Task AddSheetAsync(TreatmentSheet sheet, CancellationToken cancellationToken = default)
    {
        await dbContext.TreatmentSheets.AddAsync(sheet, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveSheetAsync(TreatmentSheet sheet, CancellationToken cancellationToken = default)
    {
        dbContext.TreatmentSheets.Update(sheet);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

