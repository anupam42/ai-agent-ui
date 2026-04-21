using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NLW.Application.Common.Interfaces;
using NLW.Application.Features.Wireframe.Commands.GenerateWireframe;
using NLW.Domain.Entities;
using NLW.Domain.Exceptions;
using NLW.Domain.ValueObjects;
using NLW.Shared.DTOs;
using Xunit;

namespace NLW.Application.Tests.Features.Wireframe;

public sealed class GenerateWireframeCommandHandlerTests
{
    private readonly Mock<IAIModelService> _aiService = new();
    private readonly Mock<IPromptBuilderService> _promptBuilder = new();
    private readonly Mock<IComponentIndexService> _componentIndex = new();
    private readonly Mock<IWireframeRepository> _repository = new();
    private readonly Mock<ILogger<GenerateWireframeCommandHandler>> _logger = new();

    private GenerateWireframeCommandHandler CreateHandler() => new(
        _aiService.Object,
        _promptBuilder.Object,
        _componentIndex.Object,
        _repository.Object,
        _logger.Object);

    [Fact]
    public async Task Handle_ValidPrompt_ReturnsDashboardSchema()
    {
        // Arrange
        var prompt = "Create a dashboard with sidebar and stats cards";
        var rootNode = new WireframeNodeDto { Type = "column", Label = "Root" };

        _promptBuilder.Setup(x => x.BuildWireframePromptAsync(prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync("system prompt");

        _aiService.Setup(x => x.GenerateWireframeAsync(prompt, "system prompt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(rootNode);

        _repository.Setup(x => x.SaveAsync(It.IsAny<WireframeSchema>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GenerateWireframeCommand(prompt), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Dashboard");
        result.Value.Root.Should().Be(rootNode);
    }

    [Fact]
    public async Task Handle_AIServiceThrows_ReturnsFailureResult()
    {
        // Arrange
        var prompt = "Build a login page";

        _promptBuilder.Setup(x => x.BuildWireframePromptAsync(prompt, It.IsAny<CancellationToken>()))
            .ReturnsAsync("system prompt");

        _aiService.Setup(x => x.GenerateWireframeAsync(prompt, "system prompt", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AIGenerationException("Model returned 429."));

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GenerateWireframeCommand(prompt), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("AI_GENERATION_FAILED");
    }

    [Theory]
    [InlineData("dashboard", "Dashboard")]
    [InlineData("login page", "Login Page")]
    [InlineData("landing homepage", "Landing Page")]
    [InlineData("registration form", "Registration Form")]
    [InlineData("user profile", "Profile Page")]
    public async Task Handle_PromptKeywords_InfersCorrectName(string prompt, string expectedName)
    {
        var rootNode = new WireframeNodeDto { Type = "column" };

        _promptBuilder.Setup(x => x.BuildWireframePromptAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("system");
        _aiService.Setup(x => x.GenerateWireframeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rootNode);
        _repository.Setup(x => x.SaveAsync(It.IsAny<WireframeSchema>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(new GenerateWireframeCommand(prompt), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be(expectedName);
    }
}
