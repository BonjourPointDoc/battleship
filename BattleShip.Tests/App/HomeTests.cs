extern alias AppAssembly;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Grpc.Net.Client;

// Utilisation de l'alias pour importer les pages/composants de l'App
using AppAssembly::BattleShip.App.Pages;

namespace BattleShip.Tests.Pages;

public class HomeTests : TestContext
{
    public HomeTests()
    {
        // Utilisation de l'alias pour le client gRPC
        var channel = GrpcChannel.ForAddress("http://localhost");
        var client = new AppAssembly::Battleship.Grpc.BattleshipGrpc.BattleshipGrpcClient(channel);

        Services.AddSingleton(client);
    }

    [Fact]
    public void Home_Should_Display_Loading_Games_Initially()
    {
        // Act
        var cut = RenderComponent<Home>();

        // Assert
        cut.Markup.Should().Contain("Chargement des parties en cours...");
    }

    [Fact]
    public void StartButton_Should_Display_Chargement_When_Clicked()
    {
        // Arrange
        var cut = RenderComponent<Home>();
        cut.WaitForState(() => !cut.Markup.Contains("Chargement des parties en cours..."));

        var startButton = cut.Find(".start-button");

        // Act
        startButton.Click();

        // Assert
        cut.Find(".start-button").TextContent.Should().Be("Chargement...");
    }
}