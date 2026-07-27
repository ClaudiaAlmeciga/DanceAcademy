using DanceAcademy.Application.Helpers;

namespace DanceAcademy.Tests.Application;

public class VideoEmbedHelperTests
{
    [Fact]
    public void Resolve_WithNullUrl_ReturnsNull()
    {
        var result = VideoEmbedHelper.Resolve(null);

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_WithEmptyUrl_ReturnsNull()
    {
        var result = VideoEmbedHelper.Resolve("");

        Assert.Null(result);
    }

    [Fact]
    public void Resolve_WithYoutubeWatchUrl_ReturnsEmbedUrl()
    {
        var result = VideoEmbedHelper.Resolve("https://www.youtube.com/watch?v=moD7Pg_HUxg");

        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com/embed/moD7Pg_HUxg", result.EmbedUrl);
        Assert.False(result.IsDirect);
    }

    [Fact]
    public void Resolve_WithYoutubeWatchUrlAndExtraParams_StripsParamsFromVideoId()
    {
        var result = VideoEmbedHelper.Resolve("https://www.youtube.com/watch?v=moD7Pg_HUxg&t=30s");

        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com/embed/moD7Pg_HUxg", result.EmbedUrl);
    }

    [Fact]
    public void Resolve_WithYoutuBeShortUrlAndTrackingParam_StripsTrackingParam()
    {
        var result = VideoEmbedHelper.Resolve("https://youtu.be/moD7Pg_HUxg?si=iVcuemjvBeYx4Tlf");

        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com/embed/moD7Pg_HUxg", result.EmbedUrl);
    }

    [Fact]
    public void Resolve_WithAlreadyEmbeddedYoutubeUrl_ReturnsSameUrl()
    {
        var result = VideoEmbedHelper.Resolve("https://www.youtube.com/embed/moD7Pg_HUxg");

        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com/embed/moD7Pg_HUxg", result.EmbedUrl);
    }

    [Fact]
    public void Resolve_WithVimeoUrl_ReturnsEmbedUrl()
    {
        var result = VideoEmbedHelper.Resolve("https://vimeo.com/123456789");

        Assert.NotNull(result);
        Assert.Equal("https://player.vimeo.com/video/123456789", result.EmbedUrl);
        Assert.False(result.IsDirect);
    }

    [Theory]
    [InlineData("https://example.com/video.mp4")]
    [InlineData("https://example.com/video.webm")]
    [InlineData("https://example.com/video.ogg")]
    public void Resolve_WithDirectVideoFile_ReturnsIsDirectTrue(string url)
    {
        var result = VideoEmbedHelper.Resolve(url);

        Assert.NotNull(result);
        Assert.Equal(url, result.EmbedUrl);
        Assert.True(result.IsDirect);
    }

    [Fact]
    public void Resolve_WithUnrecognizedUrl_ReturnsNull()
    {
        var result = VideoEmbedHelper.Resolve("https://example.com/not-a-video");

        Assert.Null(result);
    }
}
