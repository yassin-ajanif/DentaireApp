using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Models.Billing;
using DentaireApp.DataAccess.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DentaireApp.DataAccess.EFCore.Repositories;

public sealed class InvoiceRepository(AppDbContext dbContext) : IInvoiceRepository
{
    public async Task<IReadOnlyList<Invoice>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        await dbContext.Invoices
            .Include(x => x.Lines)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await dbContext.Invoices.AddAsync(invoice, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

