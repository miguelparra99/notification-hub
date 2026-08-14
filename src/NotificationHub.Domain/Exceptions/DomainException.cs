namespace NotificationHub.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(string from, string to)
        : base($"Cannot transition notification from '{from}' to '{to}'.") { }
}