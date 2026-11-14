using DocFlow.Domain.Entities;
using DocFlow.Domain.Enums;
using DocFlow.Domain.Exceptions;
using Xunit;

namespace DocFlow.Domain.Tests;

public class DocumentWorkflowTests
{
    [Fact]
    public void HappyPath_DraftToPublished_SucceedsWithCorrectStatusTransitions()
    {
        // Arrange
        var doc = DocumentTestHelper.CreateWithVersion();

        // Act & Assert
        Assert.Equal(DocumentStatus.Draft, doc.Status);

        doc.Submit();
        Assert.Equal(DocumentStatus.Submitted, doc.Status);

        doc.StartReview();
        Assert.Equal(DocumentStatus.UnderReview, doc.Status);

        doc.Approve();
        Assert.Equal(DocumentStatus.Approved, doc.Status);

        doc.Publish();
        Assert.Equal(DocumentStatus.Published, doc.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Return_NullOrEmptyOrWhitespaceReason_ThrowsDomainExceptionAndStatusUnchanged(string? reason)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.UnderReview);
        var originalStatus = doc.Status;

        // Act & Assert
        Assert.Throws<DomainException>(() => doc.Return(reason!));
        Assert.Equal(originalStatus, doc.Status);
    }

    [Fact]
    public void Return_ValidReason_TransitionsToReturnedAndStoresReasonTrimmed()
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.UnderReview);

        // Act
        doc.Return("  Changes required  ");

        // Assert
        Assert.Equal(DocumentStatus.Returned, doc.Status);
        Assert.Equal("Changes required", doc.ReturnReason);
    }

    [Fact]
    public void ReturnedDocument_CanBeResubmitted_StatusBecomesSubmittedAndReturnReasonCleared()
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(DocumentStatus.Returned);
        Assert.NotNull(doc.ReturnReason);

        // Act
        doc.Submit();

        // Assert
        Assert.Equal(DocumentStatus.Submitted, doc.Status);
        Assert.Null(doc.ReturnReason);
    }

    [Theory]
    [InlineData(DocumentStatus.Draft)]
    [InlineData(DocumentStatus.Submitted)]
    [InlineData(DocumentStatus.UnderReview)]
    [InlineData(DocumentStatus.Returned)]
    [InlineData(DocumentStatus.Approved)]
    [InlineData(DocumentStatus.Published)]
    public void Submit_InvalidStatus_ThrowsInvalidDocumentStatusTransitionException(DocumentStatus status)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(status);
        var validFromStatuses = new[] { DocumentStatus.Draft, DocumentStatus.Returned };

        if (validFromStatuses.Contains(status))
            return; // Skip valid transitions

        var originalStatus = doc.Status;
        var originalVersionCount = doc.Versions.Count;

        // Act & Assert
        Assert.Throws<InvalidDocumentStatusTransitionException>(() => doc.Submit());
        Assert.Equal(originalStatus, doc.Status);
        Assert.Equal(originalVersionCount, doc.Versions.Count);
    }

    [Theory]
    [InlineData(DocumentStatus.Draft)]
    [InlineData(DocumentStatus.Submitted)]
    [InlineData(DocumentStatus.UnderReview)]
    [InlineData(DocumentStatus.Returned)]
    [InlineData(DocumentStatus.Approved)]
    [InlineData(DocumentStatus.Published)]
    public void StartReview_InvalidStatus_ThrowsInvalidDocumentStatusTransitionException(DocumentStatus status)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(status);

        if (status != DocumentStatus.Submitted)
        {
            var originalStatus = doc.Status;
            var originalVersionCount = doc.Versions.Count;

            // Act & Assert
            Assert.Throws<InvalidDocumentStatusTransitionException>(() => doc.StartReview());
            Assert.Equal(originalStatus, doc.Status);
            Assert.Equal(originalVersionCount, doc.Versions.Count);
        }
    }

    [Theory]
    [InlineData(DocumentStatus.Draft)]
    [InlineData(DocumentStatus.Submitted)]
    [InlineData(DocumentStatus.UnderReview)]
    [InlineData(DocumentStatus.Returned)]
    [InlineData(DocumentStatus.Approved)]
    [InlineData(DocumentStatus.Published)]
    public void Return_InvalidStatus_ThrowsInvalidDocumentStatusTransitionException(DocumentStatus status)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(status);

        if (status != DocumentStatus.UnderReview)
        {
            var originalStatus = doc.Status;
            var originalVersionCount = doc.Versions.Count;

            // Act & Assert
            Assert.Throws<InvalidDocumentStatusTransitionException>(() => doc.Return("reason"));
            Assert.Equal(originalStatus, doc.Status);
            Assert.Equal(originalVersionCount, doc.Versions.Count);
        }
    }

    [Theory]
    [InlineData(DocumentStatus.Draft)]
    [InlineData(DocumentStatus.Submitted)]
    [InlineData(DocumentStatus.UnderReview)]
    [InlineData(DocumentStatus.Returned)]
    [InlineData(DocumentStatus.Approved)]
    [InlineData(DocumentStatus.Published)]
    public void Approve_InvalidStatus_ThrowsInvalidDocumentStatusTransitionException(DocumentStatus status)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(status);

        if (status != DocumentStatus.UnderReview)
        {
            var originalStatus = doc.Status;
            var originalVersionCount = doc.Versions.Count;

            // Act & Assert
            Assert.Throws<InvalidDocumentStatusTransitionException>(() => doc.Approve());
            Assert.Equal(originalStatus, doc.Status);
            Assert.Equal(originalVersionCount, doc.Versions.Count);
        }
    }

    [Theory]
    [InlineData(DocumentStatus.Draft)]
    [InlineData(DocumentStatus.Submitted)]
    [InlineData(DocumentStatus.UnderReview)]
    [InlineData(DocumentStatus.Returned)]
    [InlineData(DocumentStatus.Approved)]
    [InlineData(DocumentStatus.Published)]
    public void Publish_InvalidStatus_ThrowsInvalidDocumentStatusTransitionException(DocumentStatus status)
    {
        // Arrange
        var doc = DocumentTestHelper.AdvanceTo(status);

        if (status != DocumentStatus.Approved)
        {
            var originalStatus = doc.Status;
            var originalVersionCount = doc.Versions.Count;

            // Act & Assert
            Assert.Throws<InvalidDocumentStatusTransitionException>(() => doc.Publish());
            Assert.Equal(originalStatus, doc.Status);
            Assert.Equal(originalVersionCount, doc.Versions.Count);
        }
    }

    [Fact]
    public void StartRevision_FromEveryStatusExceptPublished_ThrowsInvalidDocumentStatusTransitionException()
    {
        // Arrange
        var invalidStatuses = new[] 
        { 
            DocumentStatus.Draft, 
            DocumentStatus.Submitted, 
            DocumentStatus.UnderReview,
            DocumentStatus.Returned,
            DocumentStatus.Approved
        };

        foreach (var status in invalidStatuses)
        {
            var doc = DocumentTestHelper.AdvanceTo(status);
            var originalStatus = doc.Status;
            var originalVersionCount = doc.Versions.Count;

            // Act & Assert
            Assert.Throws<InvalidDocumentStatusTransitionException>(() => doc.StartRevision());
            Assert.Equal(originalStatus, doc.Status);
            Assert.Equal(originalVersionCount, doc.Versions.Count);
        }
    }
}
