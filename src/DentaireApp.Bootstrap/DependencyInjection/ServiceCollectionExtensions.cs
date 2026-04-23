using DentaireApp.Bootstrap.Options;
using DentaireApp.Business.Common;
using DentaireApp.Business.Interfaces.Repositories;
using DentaireApp.Business.Interfaces.Services;
using DentaireApp.Business.Services;
using DentaireApp.DataAccess.EFCore.Persistence;
using DentaireApp.DataAccess.EFCore.Repositories;
using DentaireApp.DataAccess.EFCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

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
        services.AddSingleton<IPatientRecordService, PatientRecordService>();
        services.AddSingleton<IQueueService, QueueService>();
        services.AddSingleton<IQueuePredictionService, QueuePredictionService>();
        services.AddSingleton<IAppointmentService, AppointmentService>();
        return services;
    }

    public static IServiceCollection AddDataAccess(this IServiceCollection services, DatabaseOptions options)
    {
        services.AddDbContext<AppDbContext>(
            db =>
            {
                if (options.Provider == DatabaseProvider.SqlServer)
                {
                    db.UseSqlServer(options.ConnectionString);
                }
                else
                {
                    var sqlitePath = Path.GetFullPath(Path.Combine(
                        AppContext.BaseDirectory,
                        "..", "..", "..", "..",
                        "DentaireApp.DataAccess.EFCore",
                        "dentaire.db"));
                    db.UseSqlite($"Data Source={sqlitePath}");
                }
            },
            contextLifetime: ServiceLifetime.Singleton,
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddSingleton<IPatientRepository, PatientRepository>();
        services.AddSingleton<IAppointmentRepository, AppointmentRepository>();
        services.AddSingleton<ITreatmentInfoRepository, TreatmentInfoRepository>();
        services.AddSingleton<IAppointmentEnqueuePersistence, PatientEnqueueService>();
        return services;
    }
}
