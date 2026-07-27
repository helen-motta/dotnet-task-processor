using System.ComponentModel.DataAnnotations;
using TaskProcessor.Dtos;
using TaskProcessor.Enums;
using Xunit;

namespace TaskProcessor.UnitTests;

public class CreateTaskRequestValidationTests
{
    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotReturnErrors()
    {
        var request = new CreateTaskRequest
        {
            Type = TaskType.EnviarEmail,
            Data = "Enviar mensagem de boas-vindas"
        };

        var validationResults = Validate(request);

        Assert.Empty(validationResults);
    }

    [Fact]
    public void Validate_WhenTypeIsMissing_ShouldReturnValidationError()
    {
        var request = new CreateTaskRequest
        {
            Type = null,
            Data = "Dados válidos"
        };

        var validationResults = Validate(request);

        var error = Assert.Single(validationResults);
        Assert.Contains(nameof(CreateTaskRequest.Type), error.MemberNames);
    }

    [Fact]
    public void Validate_WhenTypeIsNotDefined_ShouldReturnValidationError()
    {
        var request = new CreateTaskRequest
        {
            Type = (TaskType)999,
            Data = "Dados válidos"
        };

        var validationResults = Validate(request);

        var error = Assert.Single(validationResults);
        Assert.Contains(nameof(CreateTaskRequest.Type), error.MemberNames);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WhenDataIsInvalid_ShouldReturnValidationError(string? data)
    {
        var request = new CreateTaskRequest
        {
            Type = TaskType.EnviarEmail,
            Data = data!
        };

        var validationResults = Validate(request);

        var error = Assert.Single(validationResults);
        Assert.Contains(nameof(CreateTaskRequest.Data), error.MemberNames);
    }

    private static List<ValidationResult> Validate(CreateTaskRequest request)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);

        Validator.TryValidateObject(
            request,
            validationContext,
            validationResults,
            validateAllProperties: true);

        return validationResults;
    }
}
