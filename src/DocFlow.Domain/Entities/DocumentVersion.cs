namespace DocFlow.Domain.Entities;

public sealed class DocumentVersion
{
    public Guid Id { get; private set; }

    public int VersionNumber { get; private set; }

    public string FileName { get; private set; }

    public string FileKey { get; private set; }

    public Guid CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsPublished { get; private set; }

    public bool IsObsolete { get; private set; }

    internal DocumentVersion(
        Guid id,
        int versionNumber,
        string fileName,
        string fileKey,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        Id = id;
        VersionNumber = versionNumber;
        FileName = fileName;
        FileKey = fileKey;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        IsPublished = false;
        IsObsolete = false;
    }

    internal void MarkPublished()
    {
        IsPublished = true;
    }

    internal void MarkObsolete()
    {
        IsObsolete = true;
    }
}
