using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace CeltiaGame.Tests;

/// <summary>
/// Custom factory that points WebApplicationFactory at the CeltiaGame project
/// directory so that wwwroot/ (with all game assets) is correctly resolved
/// whether the tests run locally or in CI.
/// </summary>
public class CeltiaWebFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(FindProjectDirectory());
    }

    private static string FindProjectDirectory()
    {
        // Walk up from the test binary output dir until we find the solution root
        // (identified by the presence of a CeltiaGame sub-folder with its .csproj).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var csproj = Path.Combine(dir.FullName, "CeltiaGame", "CeltiaGame.csproj");
            if (File.Exists(csproj))
                return Path.Combine(dir.FullName, "CeltiaGame");
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            $"Cannot locate CeltiaGame project directory. Searched up from: {AppContext.BaseDirectory}");
    }
}

/// <summary>
/// Integration tests that spin up the real ASP.NET Core host and verify the
/// game is served correctly: default document, HTML content, static asset
/// MIME types, and 404 behaviour for missing paths.
/// </summary>
public class StaticFileTests
{
    private static CeltiaWebFactory _factory = null!;
    private static HttpClient _client = null!;

    [Before(Class)]
    public static void CreateFactory()
    {
        _factory = new CeltiaWebFactory();
        _client = _factory.CreateClient();
    }

    [After(Class)]
    public static async Task DisposeFactory()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    // ── Default document ────────────────────────────────────────────────────

    [Test]
    public async Task Get_Root_Returns_200_WithHtmlContentType()
    {
        var response = await _client.GetAsync("/");

        response.EnsureSuccessStatusCode();
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("text/html");
    }

    [Test]
    public async Task Get_IndexHtml_Returns_200_WithHtmlContentType()
    {
        var response = await _client.GetAsync("/index.html");

        response.EnsureSuccessStatusCode();
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("text/html");
    }

    // ── HTML content ────────────────────────────────────────────────────────

    [Test]
    public async Task Get_Root_Html_Contains_GameCanvas()
    {
        var html = await _client.GetStringAsync("/");

        await Assert.That(html).Contains("<canvas");
        await Assert.That(html).Contains("gameCanvas");
    }

    [Test]
    public async Task Get_Root_Html_Contains_GameTitle()
    {
        var html = await _client.GetStringAsync("/");

        await Assert.That(html).Contains("Celtia");
    }

    [Test]
    public async Task Get_Root_Html_Contains_ControlsHint()
    {
        var html = await _client.GetStringAsync("/");

        await Assert.That(html).Contains("Arrow Keys");
        await Assert.That(html).Contains("Space");
    }

    // ── Sprite assets (PNG) ─────────────────────────────────────────────────

    [Test]
    [Arguments("/assets/bobs/LughAll.png")]
    [Arguments("/assets/bobs/FirBolg.png")]
    [Arguments("/assets/bobs/Fomorian.png")]
    [Arguments("/assets/bobs/BALOR_2.png")]
    [Arguments("/assets/bobs/DOMNANN.png")]
    [Arguments("/assets/bobs/EIRIC.png")]
    public async Task Get_SpriteBob_Returns_200_WithPngContentType(string path)
    {
        var response = await _client.GetAsync(path);

        response.EnsureSuccessStatusCode();
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("image/png");
    }

    [Test]
    [Arguments("/assets/backgrounds/intro.png")]
    [Arguments("/assets/backgrounds/gamewin.png")]
    [Arguments("/assets/backgrounds/arrivalback0.png")]
    public async Task Get_BackgroundImage_Returns_200_WithPngContentType(string path)
    {
        var response = await _client.GetAsync(path);

        response.EnsureSuccessStatusCode();
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsEqualTo("image/png");
    }

    // ── Sound assets (WAV) ──────────────────────────────────────────────────

    [Test]
    [Arguments("/assets/sounds/attack.wav")]
    [Arguments("/assets/sounds/damage.wav")]
    [Arguments("/assets/sounds/bossattack.wav")]
    [Arguments("/assets/sounds/bossdamage.wav")]
    [Arguments("/assets/sounds/end.wav")]
    public async Task Get_SoundAsset_Returns_200_WithAudioContentType(string path)
    {
        var response = await _client.GetAsync(path);

        response.EnsureSuccessStatusCode();
        var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        await Assert.That(mediaType).StartsWith("audio/");
    }

    // ── 404 behaviour ───────────────────────────────────────────────────────

    [Test]
    [Arguments("/nonexistent.xyz")]
    [Arguments("/assets/bobs/missing.png")]
    [Arguments("/assets/sounds/missing.wav")]
    public async Task Get_NonExistentFile_Returns_404(string path)
    {
        var response = await _client.GetAsync(path);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }
}
