using System.Net;
using Moq;
using Xunit;

namespace TestTask.ThirdParty.Tests
{
    public class RestClientDecoratorTests
    {
        [Fact]
        public async Task ExecuteWithRetry_Success()
        {
            // Arrange
            var mockOriginalRestClient = new Mock<IRestClient>();
            var mockLogger = new Mock<ILogger>();

            mockOriginalRestClient
                .Setup(client => client.Get<string>("url"))
                .ReturnsAsync("Good");

            var decorator = new RestClientDecorator(mockOriginalRestClient.Object, mockLogger.Object);

            // Act
            var result = await decorator.Get<string>("url");

            // Assert
            Assert.Equal("Good", result);
        }

        [Fact]
        public async Task ExecuteWithRetry_Fail_RetriesAndReturnsDefault()
        {
            // Arrange
            var mockOriginalRestClient = new Mock<IRestClient>();
            var mockLogger = new Mock<ILogger>();
            var retryDelay = TimeSpan.FromSeconds(1);

            mockOriginalRestClient.SetupSequence(client => client.Get<string>("url"))
                .ThrowsAsync(new WebException())
                .ThrowsAsync(new WebException())
                .ThrowsAsync(new WebException())
                .ReturnsAsync("Good");

            var decorator =
                new RestClientDecorator(mockOriginalRestClient.Object, mockLogger.Object, retryDelay: retryDelay);

            // Act
            var result = await decorator.Get<string>("url");

            // Assert
            Assert.Null(result);
            mockLogger.Verify(logger => logger.Error(It.IsAny<WebException>()), Times.Once());
        }

        [Fact]
        public async Task ExecuteWithRetry_Fail_FastFail()
        {
            // Arrange
            var mockOriginalRestClient = new Mock<IRestClient>();
            var mockLogger = new Mock<ILogger>();

            mockOriginalRestClient.Setup(client => client.Get<string>("url"))
                .ThrowsAsync(new InvalidOperationException());

            var decorator = new RestClientDecorator(mockOriginalRestClient.Object, mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.Get<string>("url"));
            mockLogger.Verify(logger => logger.Error(It.IsAny<InvalidOperationException>()), Times.Once());
        }
    }
}
