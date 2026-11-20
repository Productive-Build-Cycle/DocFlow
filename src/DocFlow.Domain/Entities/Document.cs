namespace DocFlow.Domain.Entities;

using DocFlow.Domain.Enums;
using DocFlow.Domain.Exceptions;

public sealed class Document
{
    private readonly List<DocumentVersion> _versions;

    public Guid Id { get; private set; }

    public string Code { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; } // TODO: replace with a lookup/type entity

    public Guid DepartmentId { get; private set; }

    public Guid OwnerId { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string? ReturnReason { get; private set; }

    public IReadOnlyList<DocumentVersion> Versions => _versions.AsReadOnly();

    public DocumentVersion? LatestVersion => _versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

    public DocumentVersion? CurrentValidVersion =>
        _versions.FirstOrDefault(v => v.IsPublished && !v.IsObsolete);

    private Document(
        Guid id,
        string code,
        string title,
        string type,
        Guid departmentId,
        Guid ownerId)
    {
        Id = id;
        Code = code;
        Title = title;
        Type = type;
        DepartmentId = departmentId;
        OwnerId = ownerId;
        Status = DocumentStatus.Draft;
        ReturnReason = null;
        _versions = new List<DocumentVersion>();
    }

    public static Document Create(
        string code,
        string title,
        string type,
        Guid departmentId,
        Guid ownerId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Document code must not be null, empty, or whitespace.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Document title must not be null, empty, or whitespace.");

        if (string.IsNullOrWhiteSpace(type))
            throw new DomainException("Document type must not be null, empty, or whitespace.");

        if (departmentId == Guid.Empty)
            throw new DomainException("Department ID must not be empty.");

        if (ownerId == Guid.Empty)
            throw new DomainException("Owner ID must not be empty.");

        // Code uniqueness is a persistence/application concern, not enforced at domain level
        return new Document(
            Guid.NewGuid(),
            code.Trim(),
            title.Trim(),
            type.Trim(),
            departmentId,
            ownerId);
    }

    public DocumentVersion AddVersion(
        string fileName,
        string fileKey,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (Status != DocumentStatus.Draft && Status != DocumentStatus.Returned)
            throw new DomainException(
                $"Cannot add version to document with status '{Status}'. Only documents in Draft or Returned status can have versions added.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name must not be null, empty, or whitespace.");

        if (string.IsNullOrWhiteSpace(fileKey))
            throw new DomainException("File key must not be null, empty, or whitespace.");

        if (createdBy == Guid.Empty)
            throw new DomainException("Created by must not be empty.");

        var versionNumber = (LatestVersion?.VersionNumber ?? 0) + 1;
        var newVersion = new DocumentVersion(
            Guid.NewGuid(),
            versionNumber,
            fileName.Trim(),
            fileKey.Trim(),
            createdBy,
            createdAt);

        _versions.Add(newVersion);
        return newVersion;
    }

    /// <summary>Transitions document from Draft or Returned to Submitted status.</summary>
    public void Submit()
    {
        EnsureStatus(nameof(Submit), DocumentStatus.Draft, DocumentStatus.Returned);

        if (LatestVersion == null || LatestVersion.IsPublished)
            throw new DomainException(
                "Document must have an unpublished version before submitting for review.");

        ReturnReason = null;
        Status = DocumentStatus.Submitted;
    }

    /// <summary>Transitions document from Submitted to UnderReview status.</summary>
    public void StartReview()
    {
        EnsureStatus(nameof(StartReview), DocumentStatus.Submitted);
        Status = DocumentStatus.UnderReview;
    }

    /// <summary>Transitions document from UnderReview to Returned status with a reason.</summary>
    public void Return(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Return reason must not be null, empty, or whitespace.");

        EnsureStatus(nameof(Return), DocumentStatus.UnderReview);
        ReturnReason = reason.Trim();
        Status = DocumentStatus.Returned;
    }

    /// <summary>Transitions document from UnderReview to Approved status.</summary>
    public void Approve()
    {
        EnsureStatus(nameof(Approve), DocumentStatus.UnderReview);
        Status = DocumentStatus.Approved;
    }

    /// <summary>Transitions document from Approved to Published status and marks the latest version as published.</summary>
    public void Publish()
    {
        EnsureStatus(nameof(Publish), DocumentStatus.Approved);

        CurrentValidVersion?.MarkObsolete();

        LatestVersion!.MarkPublished();
        Status = DocumentStatus.Published;
    }

    /// <summary>Transitions document from Published back to Draft status to receive a new version.</summary>
    public void StartRevision()
    {
        EnsureStatus(nameof(StartRevision), DocumentStatus.Published);
        Status = DocumentStatus.Draft;
    }

    private void EnsureStatus(string action, params DocumentStatus[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(Status))
            throw new InvalidDocumentStatusTransitionException(Status, action);
    }
}
