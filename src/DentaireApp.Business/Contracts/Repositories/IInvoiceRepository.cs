using DentaireApp.Business.Models.Billing;

namespace DentaireApp.Business.Contracts.Repositories;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
}

