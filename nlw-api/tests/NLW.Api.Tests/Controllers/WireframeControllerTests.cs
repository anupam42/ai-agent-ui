using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NLW.Api.Controllers;
using NLW.Application.Common.Models;
using NLW.Application.Features.Wireframe.Commands.GenerateWireframe;
using NLW.Shared.DTOs;
using Xunit;

namespace NLW.Api.Tests.Controllers;

public sealed class WireframeControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    private WireframeController CreateController() => new(_mediator.Object);

    [Fact]
    public async Task Generate_ValidRequest_Returns200WithSchema()
    {
        // Arrange
        var response = new GenerateWireframeResponse
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Dashboard",
            Prompt = "Create a dashboard",
            Root = new WireframeNodeDto { Type = "column" },
            CreatedAt = DateTime.UtcNow
        };

        _mediator.Setup(m => m.Send(It.IsAny<GenerateWireframeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GenerateWireframeResponse>.Success(response));

        var controller = CreateController();
        var request = new GenerateWireframeRequest { Prompt = "Create a dashboard" };

        // Act
        var result = await controller.Generate(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task Generate_AIFailure_Returns502()
    {
        // Arrange
        _mediator.Setup(m => m.Send(It.IsAny<GenerateWireframeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GenerateWireframeResponse>.Failure("AI_GENERATION_FAILED", "Claude returned 429"));

        var controller = CreateController();
        var request = new GenerateWireframeRequest { Prompt = "Create a dashboard" };

        // Act
        var result = await controller.Generate(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)result).StatusCode.Should().Be(502);
    }
}
