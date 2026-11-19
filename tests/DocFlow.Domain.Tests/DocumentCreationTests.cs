using DocFlow.Domain.Entities;
using DocFlow.Domain.Exceptions;
using Xunit;

namespace DocFlow.Domain.Tests;

public class DocumentCreationTests
{
    private readonly Guid _departmentId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _ownerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void Create_ValidData_ReturnsDocumentInDraftWithNoVersions()
    {
        // Arrange & Act
        var doc = Document.Create("DOC-001", "Test Document", "Report", _departmentId, _ownerId);

        // Assert
        Assert.NotEqual(Guid.Empty, doc.Id);
        Assert.Equal("DOC-001", doc.Code);
        Assert.Equal("Test Document", doc.Title);
        Assert.Equal("Report", doc.Type);
        Assert.Equal(_departmentId, doc.DepartmentId);
        Assert.Equal(_ownerId, doc.OwnerId);
        Assert.Equal(Enums.DocumentStatus.Draft, doc.Status);
        Assert.Null(doc.ReturnReason);
        Assert.Empty(doc.Versions);
        Assert.Null(doc.LatestVersion);
        Assert.Null(doc.CurrentValidVersion);
    }

    [Fact]
    public void Create_CodeWithWhitespace_TrimmedBeforeStorage()
    {
        // Arrange & Act
        var doc = Document.Create("  DOC-001  ", "Test Document", "Report", _departmentId, _ownerId);

        // Assert
        Assert.Equal("DOC-001", doc.Code);
    }

    [Fact]
    public void Create_TitleWithWhitespace_TrimmedBeforeStorage()
    {
        // Arrange & Act
        var doc = Document.Create("DOC-001", "  Test Document  ", "Report", _departmentId, _ownerId);

        // Assert
        Assert.Equal("Test Document", doc.Title);
    }

    [Fact]
    public void Create_TypeWithWhitespace_TrimmedBeforeStorage()
    {
        // Arrange & Act
        var doc = Document.Create("DOC-001", "Test Document", "  Report  ", _departmentId, _ownerId);

        // Assert
        Assert.Equal("Report", doc.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyOrWhitespaceCode_ThrowsDomainException(string? code)
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() =>
            Document.Create(code!, "Test Document", "Report", _departmentId, _ownerId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyOrWhitespaceTitle_ThrowsDomainException(string? title)
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() =>
            Document.Create("DOC-001", title!, "Report", _departmentId, _ownerId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrEmptyOrWhitespaceType_ThrowsDomainException(string? type)
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() =>
            Document.Create("DOC-001", "Test Document", type!, _departmentId, _ownerId));
    }

    [Fact]
    public void Create_EmptyDepartmentId_ThrowsDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() =>
            Document.Create("DOC-001", "Test Document", "Report", Guid.Empty, _ownerId));
    }

    [Fact]
    public void Create_EmptyOwnerId_ThrowsDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() =>
            Document.Create("DOC-001", "Test Document", "Report", _departmentId, Guid.Empty));
    }
}
