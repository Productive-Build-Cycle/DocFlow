using DocFlow.Domain.Entities;
using DocFlow.Domain.Enums;
using DocFlow.Domain.Exceptions;
using Xunit;

namespace DocFlow.Domain.Tests;

public class DocumentPublishingTests
{
    private readonly Guid _createdBy = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly DateTimeOffset _fixedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Submit_WithNoVersion_ThrowsDomainException()
    {
        // Arrange
        var doc = DocumentTestHelper.CreateDraft();

        // Act & Assert
        Assert.Throws<DomainException>(() => doc.Submit());
    }

    [Fact]
    public void Submit_WhenLatestVersionIsPublished_ThrowsDomainException()
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.Published);

        // Act: Transition back to Draft via StartRevision
        doc.StartRevision();

        // Act & Assert: Try to submit without adding a new version
        Assert.Throws<DomainException>(() => doc.Submit());
    }

    [Fact]
    public void Publish_FirstVersion_MarksAsPublishedAndNotObsolete()
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.Approved);
        var latestVersion = doc.LatestVersion;
        Assert.NotNull(latestVersion);

        // Act
        doc.Publish();

        // Assert
        Assert.True(latestVersion.IsPublished);
        Assert.False(latestVersion.IsObsolete);
        Assert.Equal(latestVersion, doc.CurrentValidVersion);
    }

    [Fact]
    public void Publish_SecondCycle_ObsoletesFirstVersionButKeepsInVersions()
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.Published);
        var v1 = doc.LatestVersion;
        Assert.NotNull(v1);

        // Act: Revision cycle
        doc.StartRevision();
        var v2 = doc.AddVersion("file2.pdf", "storage/file2.pdf", _createdBy, _fixedDate);
        doc.Submit();
        doc.StartReview();
        doc.Approve();
        doc.Publish();

        // Assert
        Assert.True(v1.IsPublished);
        Assert.True(v1.IsObsolete);
        Assert.True(v2.IsPublished);
        Assert.False(v2.IsObsolete);
        Assert.Contains(v1, doc.Versions);
        Assert.Contains(v2, doc.Versions);
        Assert.Equal(2, doc.Versions.Count);
        Assert.Equal(v2, doc.CurrentValidVersion);
    }

    [Fact]
    public void WhileV2InReview_V1RemainsCurrentValidVersionAndNotObsolete()
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.Published);
        var v1 = doc.LatestVersion;
        Assert.NotNull(v1);

        // Act: Start revision but don't publish v2 yet
        doc.StartRevision();
        var v2 = doc.AddVersion("file2.pdf", "storage/file2.pdf", _createdBy, _fixedDate);
        doc.Submit();
        doc.StartReview();

        // Assert: v1 should still be the valid version
        Assert.True(v1.IsPublished);
        Assert.False(v1.IsObsolete);
        Assert.False(v2.IsPublished);
        Assert.False(v2.IsObsolete);
        Assert.Equal(v1, doc.CurrentValidVersion);
    }

    [Fact]
    public void PublishingCycle_InvariantExactlyOneValidVersion_HeldThroughoutProcess()
    {
        // Arrange
        var doc = DocumentTestHelper.CreateWithVersion();

        // Helper function to count valid versions
        int CountValidVersions() => doc.Versions.Count(v => v.IsPublished && !v.IsObsolete);

        // Act & Assert: Draft -> Submitted
        doc.Submit();
        Assert.True(CountValidVersions() <= 1);

        // Act & Assert: Submitted -> UnderReview
        doc.StartReview();
        Assert.True(CountValidVersions() <= 1);

        // Act & Assert: UnderReview -> Approved
        doc.Approve();
        Assert.True(CountValidVersions() <= 1);

        // Act & Assert: Approved -> Published
        doc.Publish();
        Assert.Equal(1, CountValidVersions());
        var v1 = doc.LatestVersion;
        Assert.NotNull(v1);

        // Act & Assert: Published -> Draft (revision)
        doc.StartRevision();
        Assert.Equal(1, CountValidVersions());

        // Act & Assert: Add v2
        var v2 = doc.AddVersion("file2.pdf", "storage/file2.pdf", _createdBy, _fixedDate);
        Assert.Equal(1, CountValidVersions());

        // Act & Assert: Draft -> Submitted
        doc.Submit();
        Assert.Equal(1, CountValidVersions());

        // Act & Assert: Submitted -> UnderReview
        doc.StartReview();
        Assert.Equal(1, CountValidVersions());

        // Act & Assert: UnderReview -> Approved
        doc.Approve();
        Assert.Equal(1, CountValidVersions());

        // Act & Assert: Approved -> Published (v2 becomes valid, v1 becomes obsolete)
        doc.Publish();
        Assert.Equal(1, CountValidVersions());
        Assert.True(v1.IsObsolete);
        Assert.False(v2.IsObsolete);
    }
}
