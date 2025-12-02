using FluentAssertions;
using SdJwt.Net.StatusList.Verifier;
using System;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Exceptions;

public class StatusListExceptionTests
{
    [Fact]
    public void StatusListFetchException_WithMessage_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Failed to fetch status list";

        // Act
        var exception = new StatusListFetchException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void StatusListFetchException_WithMessageAndInnerException_ShouldCreateCorrectly()
    {
        // Arrange
        var message = "Failed to fetch status list";
        var innerException = new HttpRequestException("Network error");

        // Act
        var exception = new StatusListFetchException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void StatusListFetchException_ShouldBeAssignableToException()
    {
        // Arrange
        var exception = new StatusListFetchException("test message");

        // Act & Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}

// Helper class if HttpRequestException is not available
public class HttpRequestException : Exception
{
    public HttpRequestException(string message) : base(message) { }
}
