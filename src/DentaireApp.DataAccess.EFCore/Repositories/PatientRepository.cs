using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Models.Patients;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DentaireApp.DataAccess.EFCore.Repositories;

public sealed class PatientRepository(AppDbContext dbContext) : IPatientRepository
{
    public Task<Patient?> GetByIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        dbContext.Patients
            .Include(x => x.TreatmentInfos)
            .FirstOrDefaultAsync(x => x.Id == patientId, cancellationToken);

    public async Task<IReadOnlyList<Patient>> GetByIdsAsync(
        IReadOnlyCollection<Guid> patientIds,
        CancellationToken cancellationToken = default)
    {
        if (patientIds.Count == 0)
        {
            return [];
        }

        var distinct = patientIds.Distinct().ToArray();
        return await dbContext.Patients
            .Where(p => distinct.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Patients
            .OrderBy(x => x.Nom)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPatientsPageAsync(
        int skip,
        int take,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Patients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var t = searchTerm.Trim();
            var tLower = t.ToLowerInvariant();
            query = query.Where(p =>
                p.Nom.ToLower().Contains(tLower) ||
                p.Telephone.ToLower().Contains(tLower));
        }

        query = query.OrderBy(p => p.Nom);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<Patient?> GetByTelephoneAsync(string telephone, CancellationToken cancellationToken = default) =>
        dbContext.Patients.FirstOrDefaultAsync(x => x.Telephone == telephone, cancellationToken);

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await dbContext.Patients.AddAsync(patient, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        dbContext.Patients.Update(patient);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeletePatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Patients.AnyAsync(x => x.Id == patientId, cancellationToken);
        if (!exists)
        {
            return false;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await dbContext.TreatmentInfos
                .Where(t => t.PatientId == patientId)
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.Appointments
                .Where(a => a.PatientId == patientId)
                .ExecuteDeleteAsync(cancellationToken);
            await dbContext.Patients
                .Where(p => p.Id == patientId)
                .ExecuteDeleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

