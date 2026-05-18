namespace ProductAPI.Domain.Exceptions;

public class DomainValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public DomainValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}