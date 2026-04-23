using DentaireApp.Bootstrap.Options;
using DentaireApp.Business.Common;
using DentaireApp.Business.Contracts.Repositories;
using DentaireApp.Business.Contracts.Services;
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
                // Anchor to repo layout: .../UI.Avalonia/bin/Debug/net9.0/ -> .../src/DentaireApp.DataAccess.EFCore/dentaire.db
                // (avoids a second empty DB next to the UI project / wrong process CWD.)
                var sqlitePath = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..", "..", "..", "..",
                    "DentaireApp.DataAccess.EFCore",
                    "dentaire.db"));
                db.UseSqlite($"Data Source={sqlitePath}");
            }
        });

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ITreatmentInfoRepository, TreatmentInfoRepository>();
        services.AddScoped<IPatientEnqueueService, PatientEnqueueService>();
        return services;
    }
}

