using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NotificationHub.Application.Notifications.Dtos;
using NotificationHub.Application.Notifications.Services;

namespace NotificationHub.Api.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly IValidator<SendNotificationRequest> _validator;

    public NotificationsController(INotificationService service, IValidator<SendNotificationRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Queues and dispatches a notification.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var result = await _service.SendAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Retrieves a notification and its delivery history.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}