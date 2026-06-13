namespace InventoryApi.Domain.Exceptions;

/// <summary>
/// Thrown when a domain rule or invariant is violated.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
