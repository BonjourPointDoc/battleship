// 1. Déclarer l'alias tout en haut du fichier
extern alias AppAssembly;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
// 2. Ajouter le bon using pour trouver le composant "Home"
using BattleShip.App.Pages; // (Ajustez ".Pages" selon le dossier où se trouve Home.razor)
using Grpc.Net.Client;
namespace BattleShip.Tests.Pages;

public class HomeTests : TestContext
{
    public HomeTests()
    {
        // 3. Utiliser l'alias AppAssembly pour lever l'ambiguïté sur BattleshipGrpcClient
        var channel = GrpcChannel.ForAddress("http://localhost");
        var client = new AppAssembly::Battleship.Grpc.BattleshipGrpc.BattleshipGrpcClient(channel);

        Services.AddSingleton(client);
    }

    [Fact]
    public void Home_Should_Display_Loading_Games_Initially()
    {
        // Act
        var cut = RenderComponent<Home>(); //[cite: 1]

        // Assert
        cut.Markup.Should().Contain("Chargement des parties en cours..."); //[cite: 1]
    }

    [Fact]
    public void StartButton_Should_Display_Chargement_When_Clicked()
    {
        // Arrange
        var cut = RenderComponent<Home>(); //[cite: 1]
        cut.WaitForState(() => !cut.Markup.Contains("Chargement des parties en cours..."));

        var startButton = cut.Find(".start-button"); //[cite: 1]

        // Act
        startButton.Click();

        // Assert
        cut.Find(".start-button").TextContent.Should().Be("Chargement..."); //[cite: 1]
    }
}