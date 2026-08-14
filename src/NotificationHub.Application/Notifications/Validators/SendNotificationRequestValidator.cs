using FluentValidation;
using NotificationHub.Application.Notifications.Dtos;
using NotificationHub.Domain.Enums;

namespace NotificationHub.Application.Notifications.Validators;

public class SendNotificationRequestValidator : AbstractValidator<SendNotificationRequest>
{
    public SendNotificationRequestValidator()
    {
        RuleFor(x => x.Channel).IsInEnum();

        RuleFor(x => x.Recipient)
            .NotEmpty()
            .MaximumLength(320);

        RuleFor(x => x.Recipient)
            .EmailAddress()
            .When(x => x.Channel == NotificationChannel.Email);

        RuleFor(x => x.Recipient)
            .Matches(@"^\+?[0-9]{7,15}$")
            .When(x => x.Channel == NotificationChannel.Sms)
            .WithMessage("Recipient must be a valid phone number.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Body) || !string.IsNullOrWhiteSpace(x.TemplateCode))
            .WithMessage("Either 'body' or 'templateCode' is required.");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .When(x => x.Channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(x.TemplateCode))
            .WithMessage("Email notifications require a subject.");
    }
}