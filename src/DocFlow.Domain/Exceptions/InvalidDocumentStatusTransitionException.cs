using DocFlow.Domain.Enums;

namespace DocFlow.Domain.Exceptions;

public sealed class InvalidDocumentStatusTransitionException : DomainException
{
    public DocumentStatus From { get; }
    public string Action { get; }

    public InvalidDocumentStatusTransitionException(DocumentStatus from, string action)
        : base($"Cannot perform '{action}' when document status is '{from}'.")
    {
        From = from;
        Action = action;
    }
}
