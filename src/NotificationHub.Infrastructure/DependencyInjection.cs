using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Domain.Enums;
using NotificationHub.Infrastructure.Persistence;
using NotificationHub.Infrastructure.Persistence.Repositories;
using NotificationHub.Infrastructure.Senders;
using Microsoft.Extensions.Options;

namespace NotificationHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();

        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        services.AddScoped<INotificationSender, SmtpEmailSender>();

        services.AddScoped<INotificationSender>(sp => new SimulatedSender(
            NotificationChannel.Sms, "SimulatedSms", sp.GetRequiredService<ILogger<SimulatedSender>>()));

        services.AddScoped<INotificationSender>(sp => new SimulatedSender(
            NotificationChannel.Push, "SimulatedPush", sp.GetRequiredService<ILogger<SimulatedSender>>()));

        services.AddScoped<ISenderResolver, SenderResolver>();

        return services;
    }
}