using DentaireApp.Bootstrap.Options;
using DentaireApp.Business.Common;
using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
using DentaireApp.Business.Services;
using DentaireApp.DataAccess.EFCore.Persistence;
using DentaireApp.DataAccess.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DentaireApp.Bootstrap.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDentaireCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<QueuePredictionOptions>(configuration.GetSection(QueuePredictionOptions.SectionName));

        services.AddBusinessServices();
        services.AddDataAccess(configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions());
        return services;
    }

    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IPatientRegistrationService, PatientRegistrationService>();
        services.AddScoped<IPatientRecordService, PatientRecordService>();
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<IQueuePredictionService, QueuePredictionService>();
        return services;
    }

    public static IServiceCollection AddDataAccess(this IServiceCollection services, DatabaseOptions options)
    {
        services.AddDbContext<AppDbContext>(db =>
        {
            if (options.Provider == DatabaseProvider.SqlServer)
            {
                db.UseSqlServer(options.ConnectionString);
            }
            else
            {
                db.UseSqlite(options.ConnectionString);
            }
        });

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ITreatmentRepository, TreatmentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        return services;
    }
}

