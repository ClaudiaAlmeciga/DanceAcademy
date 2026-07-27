namespace DanceAcademy.Application.Helpers;

public static class VideoEmbedHelper
{
    public sealed record VideoEmbedResult(string EmbedUrl, bool IsDirect);

    public static VideoEmbedResult? Resolve(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        if (url.Contains("youtube.com/embed/") || url.Contains("player.vimeo.com/video/"))
            return new VideoEmbedResult(url, false);

        if (url.Contains("youtube.com/watch") && url.Contains("v="))
        {
            var vIdx = url.IndexOf("v=", StringComparison.Ordinal) + 2;
            var videoId = url[vIdx..];
            var ampIdx = videoId.IndexOf('&');
            if (ampIdx >= 0) videoId = videoId[..ampIdx];
            return new VideoEmbedResult($"https://www.youtube.com/embed/{videoId}", false);
        }

        if (url.Contains("youtu.be/"))
        {
            var idx = url.IndexOf("youtu.be/", StringComparison.Ordinal) + "youtu.be/".Length;
            var videoId = url[idx..];
            var qIdx = videoId.IndexOf('?');
            if (qIdx >= 0) videoId = videoId[..qIdx];
            return new VideoEmbedResult($"https://www.youtube.com/embed/{videoId}", false);
        }

        if (url.Contains("vimeo.com/"))
        {
            var lastSlash = url.TrimEnd('/').LastIndexOf('/');
            if (lastSlash >= 0)
            {
                var videoId = url[(lastSlash + 1)..].Split('?')[0].TrimEnd('/');
                if (!string.IsNullOrEmpty(videoId) && videoId.All(char.IsDigit))
                    return new VideoEmbedResult($"https://player.vimeo.com/video/{videoId}", false);
            }
        }

        var lower = url.ToLowerInvariant();
        if (lower.EndsWith(".mp4") || lower.EndsWith(".webm") || lower.EndsWith(".ogg"))
            return new VideoEmbedResult(url, true);

        return null;
    }
}
