using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NotificationHub.Api.Middleware;
using NotificationHub.Application.Notifications.Services;
using NotificationHub.Application.Notifications.Validators;
using NotificationHub.Infrastructure;
using NotificationHub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddValidatorsFromAssemblyContaining<SendNotificationRequestValidator>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await context.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }