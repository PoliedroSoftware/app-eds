using NUnit.Framework;
using System.Text.Json;

namespace Mobile_tests
{
    // Simplified ErrorResponse model for testing
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    [TestFixture]
    public class ProductValidationErrorTest
    {
        [Test]
        [Category("ProductValidation")]
        public void ShouldDetectValidationFailedErrorType()
        {
            // Arrange - Simulate the error response from the backend
            var errorJson = @"{
                ""type"": ""ValidationFailed"",
                ""status"": 400,
                ""detail"": ""System.Collections.Generic.List`1[FluentValidation.Results.ValidationFailure]""
            }";

            // Act - Parse the error response
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            // Assert
            Assert.That(errorResponse, Is.Not.Null, "Should parse error response");
            Assert.That(errorResponse!.Type, Is.EqualTo("ValidationFailed"), "Should detect ValidationFailed type");
            Assert.That(errorResponse.Status, Is.EqualTo(400), "Should have status 400");
            Assert.That(errorResponse.Detail, Does.Contain("FluentValidation"), "Should contain FluentValidation in detail");
        }

        [Test]
        [Category("ProductValidation")]
        public void ShouldDetectFluentValidationInDetail()
        {
            // Arrange
            var detail = "System.Collections.Generic.List`1[FluentValidation.Results.ValidationFailure]";

            // Act
            var containsFluentValidation = detail.Contains("FluentValidation");
            var containsListError = detail.Contains("System.Collections.Generic.List");

            // Assert
            Assert.That(containsFluentValidation, Is.True, "Should detect FluentValidation in detail");
            Assert.That(containsListError, Is.True, "Should detect List serialization error");
        }

        [Test]
        [Category("ProductValidation")]
        public void ShouldParseValidErrorResponseWithTitle()
        {
            // Arrange - Error response with title
            var errorJson = @"{
                ""type"": ""ValidationFailed"",
                ""status"": 400,
                ""title"": ""One or more validation errors occurred."",
                ""detail"": ""System.Collections.Generic.List`1[FluentValidation.Results.ValidationFailure]""
            }";

            // Act
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            // Assert
            Assert.That(errorResponse, Is.Not.Null);
            Assert.That(errorResponse!.Type, Is.EqualTo("ValidationFailed"));
            Assert.That(errorResponse.Title, Does.Contain("validation"));
        }

        [Test]
        [Category("ProductValidation")]
        public void ShouldHandleNullOrEmptyErrorDetail()
        {
            // Arrange
            var errorResponse1 = new ErrorResponse { Type = "ValidationFailed", Status = 400, Detail = null! };
            var errorResponse2 = new ErrorResponse { Type = "ValidationFailed", Status = 400, Detail = "" };

            // Act
            var detail1 = errorResponse1.Detail ?? string.Empty;
            var detail2 = errorResponse2.Detail ?? string.Empty;

            // Assert
            Assert.That(detail1, Is.Empty, "Should handle null detail");
            Assert.That(detail2, Is.Empty, "Should handle empty detail");
        }

        [Test]
        [Category("ProductValidation")]
        public void ShouldIdentifyValidationFailedFromMultipleCriteria()
        {
            // Arrange
            var errorResponse = new ErrorResponse 
            { 
                Type = "ValidationFailed", 
                Status = 400, 
                Detail = "System.Collections.Generic.List`1[FluentValidation.Results.ValidationFailure]" 
            };

            // Act - Check multiple conditions that should trigger ValidationFailed handling
            var isValidationFailedByType = errorResponse.Type == "ValidationFailed";
            var isValidationFailedByDetail = errorResponse.Detail?.Contains("FluentValidation") == true;
            var isValidationFailedByListError = errorResponse.Detail?.Contains("System.Collections.Generic.List") == true;

            // Assert
            Assert.That(isValidationFailedByType, Is.True, "Should detect by Type property");
            Assert.That(isValidationFailedByDetail, Is.True, "Should detect by FluentValidation in detail");
            Assert.That(isValidationFailedByListError, Is.True, "Should detect by List serialization error");
        }

        [Test]
        [Category("ProductValidation")]
        public void ShouldHandlePartialErrorResponse()
        {
            // Arrange - Minimal error response
            var errorJson = @"{""type"": ""ValidationFailed""}";

            // Act
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            // Assert
            Assert.That(errorResponse, Is.Not.Null);
            Assert.That(errorResponse!.Type, Is.EqualTo("ValidationFailed"));
            Assert.That(errorResponse.Status, Is.EqualTo(0), "Status should default to 0");
            Assert.That(errorResponse.Detail, Is.Empty, "Detail should be empty string");
        }
    }
}
