using DocFlow.Domain.Entities;
using DocFlow.Domain.Enums;

namespace DocFlow.Domain.Tests;

public static class DocumentTestHelper
{
    private static readonly Guid DefaultDepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DefaultOwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DefaultCreatedBy = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly DateTimeOffset FixedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>Creates a document in Draft status with no versions.</summary>
    public static Document CreateDraft()
    {
        return Document.Create("DOC-001", "Test Document", "Report", DefaultDepartmentId, DefaultOwnerId);
    }

    /// <summary>Creates a document in Draft status with one version.</summary>
    public static Document CreateWithVersion()
    {
        var doc = CreateDraft();
        doc.AddVersion("file.pdf", "storage/file.pdf", DefaultCreatedBy, FixedDate);
        return doc;
    }

    /// <summary>Advances a document to the specified status through valid state transitions.</summary>
    public static Document AdvanceTo(DocumentStatus targetStatus)
    {
        var doc = CreateWithVersion();

        return targetStatus switch
        {
            DocumentStatus.Draft => doc,
            DocumentStatus.Submitted => SubmitDoc(doc),
            DocumentStatus.UnderReview => ReviewDoc(SubmitDoc(doc)),
            DocumentStatus.Returned => ReturnDoc(ReviewDoc(SubmitDoc(doc))),
            DocumentStatus.Approved => ApproveDoc(ReviewDoc(SubmitDoc(doc))),
            DocumentStatus.Published => PublishDoc(ApproveDoc(ReviewDoc(SubmitDoc(doc)))),
            _ => throw new ArgumentException($"Cannot advance to status {targetStatus}")
        };
    }

    private static Document SubmitDoc(Document doc)
    {
        doc.Submit();
        return doc;
    }

    private static Document ReviewDoc(Document doc)
    {
        doc.StartReview();
        return doc;
    }

    private static Document ReturnDoc(Document doc)
    {
        doc.Return("Changes required");
        return doc;
    }

    private static Document ApproveDoc(Document doc)
    {
        doc.Approve();
        return doc;
    }

    private static Document PublishDoc(Document doc)
    {
        doc.Publish();
        return doc;
    }
}
