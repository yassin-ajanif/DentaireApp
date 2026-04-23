using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Models.Patients;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Repositories;

public sealed class TreatmentInfoRepository(AppDbContext dbContext) : ITreatmentInfoRepository
{
    public async Task<IReadOnlyList<TreatmentInfo>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        await dbContext.TreatmentInfos
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.Date)
            .ToListAsync(cancellationToken);

    public Task<TreatmentInfo?> GetByIdAsync(Guid treatmentInfoId, CancellationToken cancellationToken = default) =>
        dbContext.TreatmentInfos.FirstOrDefaultAsync(x => x.Id == treatmentInfoId, cancellationToken);

    public async Task AddAsync(TreatmentInfo treatmentInfo, CancellationToken cancellationToken = default)
    {
        await dbContext.TreatmentInfos.AddAsync(treatmentInfo, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(TreatmentInfo treatmentInfo, CancellationToken cancellationToken = default)
    {
        dbContext.TreatmentInfos.Update(treatmentInfo);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceByPatientIdAsync(Guid patientId, IReadOnlyList<TreatmentInfo> treatmentInfos, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.TreatmentInfos
            .Where(x => x.PatientId == patientId)
            .ToListAsync(cancellationToken);
        dbContext.TreatmentInfos.RemoveRange(existing);

        foreach (var info in treatmentInfos)
        {
            info.PatientId = patientId;
        }

        await dbContext.TreatmentInfos.AddRangeAsync(treatmentInfos, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

