namespace TMS.Domain.Exceptions;

public class DomainRuleException : Exception
{
    public DomainRuleException(string message) : base(message) { }
}
