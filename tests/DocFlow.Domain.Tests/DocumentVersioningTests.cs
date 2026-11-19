using DocFlow.Domain.Entities;
using DocFlow.Domain.Enums;
using DocFlow.Domain.Exceptions;
using Xunit;

namespace DocFlow.Domain.Tests;

public class DocumentVersioningTests
{
    private readonly Guid _createdBy = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly DateTimeOffset _fixedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddVersion_MultipleVersions_IncrementNumbersCorrectly()
    {
        // Arrange
        var doc = DocumentTestHelper.CreateDraft();

        // Act
        var v1 = doc.AddVersion("file1.pdf", "storage/file1.pdf", _createdBy, _fixedDate);
        var v2 = doc.AddVersion("file2.pdf", "storage/file2.pdf", _createdBy, _fixedDate);
        var v3 = doc.AddVersion("file3.pdf", "storage/file3.pdf", _createdBy, _fixedDate);

        // Assert
        Assert.Equal(1, v1.VersionNumber);
        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal(3, v3.VersionNumber);
        Assert.Equal(3, doc.Versions.Count);
        Assert.Contains(v1, doc.Versions);
        Assert.Contains(v2, doc.Versions);
        Assert.Contains(v3, doc.Versions);
    }

    [Fact]
    public void AddVersion_EarlierVersionsRemainUnchanged()
    {
        // Arrange
        var doc = DocumentTestHelper.CreateDraft();
        var v1 = doc.AddVersion("file1.pdf", "storage/file1.pdf", _createdBy, _fixedDate);
        var v1IsPublishedBefore = v1.IsPublished;

        // Act
        var v2 = doc.AddVersion("file2.pdf", "storage/file2.pdf", _createdBy, _fixedDate);

        // Assert
        Assert.Equal(v1IsPublishedBefore, v1.IsPublished);
        Assert.Equal("file1.pdf", v1.FileName);
        Assert.Equal("storage/file1.pdf", v1.FileKey);
        Assert.Equal(1, v1.VersionNumber);
    }

    [Theory]
    [InlineData(DocumentStatus.Submitted)]
    [InlineData(DocumentStatus.UnderReview)]
    [InlineData(DocumentStatus.Approved)]
    [InlineData(DocumentStatus.Published)]
    public void AddVersion_WhenStatusNotDraftOrReturned_ThrowsDomainException(DocumentStatus status)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(status);

        // Act & Assert
        Assert.Throws<DomainException>(() =>
            doc.AddVersion("file.pdf", "storage/file.pdf", _createdBy, _fixedDate));
    }

    [Fact]
    public void Versions_IsReadOnly_CannotAddDirectly()
    {
        // Arrange
        var doc = DocumentTestHelper.CreateWithVersion();
        var versions = doc.Versions;

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
        {
            var list = (System.Collections.Generic.IList<DocumentVersion>)versions;
            var dummy = doc.AddVersion("dummy.pdf", "storage/dummy.pdf", _createdBy, _fixedDate);
            list.Add(dummy);
        });
    }
}
