using System.Text.RegularExpressions;
using NotificationHub.Domain.Common;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Domain.Entities;

public class NotificationTemplate : BaseEntity
{
    private static readonly Regex PlaceholderPattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public NotificationChannel Channel { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private NotificationTemplate() { } // EF Core

    public static NotificationTemplate Create(
        string code, string name, NotificationChannel channel, string body, string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Template code is required.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Template body is required.");
        if (channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Email templates require a subject.");

        return new NotificationTemplate
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Channel = channel,
            Body = body,
            Subject = subject,
            IsActive = true
        };
    }

    /// <summary>Replaces {{placeholders}} with the supplied values.</summary>
    public string Render(IReadOnlyDictionary<string, string> values)
    {
        var missing = RequiredPlaceholders().Except(values.Keys).ToList();
        if (missing.Count > 0)
            throw new DomainException($"Missing template values: {string.Join(", ", missing)}.");

        return PlaceholderPattern.Replace(Body, m => values[m.Groups[1].Value]);
    }

    public IEnumerable<string> RequiredPlaceholders() =>
        PlaceholderPattern.Matches(Body).Select(m => m.Groups[1].Value).Distinct();

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}