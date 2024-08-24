using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Twitch.Common.Models.Api;

public class CPHInline
{
    private List<int> availableIndices;
    private string nextVideoUrl;
    private string nextClipInfo;
    private double nextClipDuration;

    public bool Execute()
    {
        // Fetch global variables set in Streamer.bot
        var scene = CPH.GetGlobalVar<string>("BRBScene", true);
        var source = CPH.GetGlobalVar<string>("BRBBrowserSource", true);
        var clipCreditsSource = CPH.GetGlobalVar<string>("ClipCreditsSource", true);
        var videoPlayerHtml = CPH.GetGlobalVar<string>("VideoPlayerHtml", true);
        var nodeServerUrl = CPH.GetGlobalVar<string>("NodeServerUrl", true);
        var nodeServerPort = CPH.GetGlobalVar<string>("NodeServerPort", true);
        var workingDirectory = CPH.GetGlobalVar<string>("WorkingDirectory", true);
        List<ClipData> allClips = JsonConvert.DeserializeObject<List<ClipData>>(CPH.GetGlobalVar<string>("BrbAllClips", true));

        // Validate the nodeServerPort, default to 3000 if not set or invalid
        if (string.IsNullOrEmpty(nodeServerPort) || !int.TryParse(nodeServerPort, out int port) || port <= 0 || port > 65535)
        {
            CPH.LogWarn($"Invalid or missing NodeServerPort. Defaulting to port 3000.");
            nodeServerPort = "3000";
        }

        string fullNodeServerUrl = $"http://{nodeServerUrl}:{nodeServerPort}";
        CPH.LogWarn("Server URL: " + fullNodeServerUrl);

        // Initialize available indices if empty
        if (availableIndices == null || availableIndices.Count <= 0)
        {
            availableIndices = new List<int>();
            for (int i = 0; i < allClips.Count; i++)
            {
                availableIndices.Add(i);
                // CPH.LogWarn("All Clips Title: " + allClips[i].Title + " | Index: " + i);
            }
        }

        Random rd = new Random();

        // Start fetching the first video URL asynchronously
        Task fetchNextVideoTask = FetchNextVideoUrlAsync(allClips, rd, videoPlayerHtml, fullNodeServerUrl, clipCreditsSource, scene, workingDirectory);

        // Set OBS scene to BRB scene
        CPH.ObsSetScene(scene);
        int delayLoop = 0;
        while ((delayLoop < 25) && (CPH.ObsGetCurrentScene() != scene))
        {
            CPH.Wait(100);
            delayLoop++;
        }

        while (CPH.ObsGetCurrentScene() == scene)
        {
            // Wait for the next video URL to be ready
            fetchNextVideoTask.Wait();

            if (!string.IsNullOrEmpty(nextVideoUrl))
            {
                CPH.LogWarn("Setting OBS Browser Source to: " + nextVideoUrl);
                CPH.ObsSetBrowserSource(scene, source, nextVideoUrl);
                CPH.ObsSetGdiText(scene, clipCreditsSource, nextClipInfo);
            }

            // Set delay based on the clip's duration + 500ms for safety
            int delay = (int)(nextClipDuration * 1000) + 500;

            // Fetch the next video URL while the current video is playing
            fetchNextVideoTask = FetchNextVideoUrlAsync(allClips, rd, videoPlayerHtml, fullNodeServerUrl, clipCreditsSource, scene, workingDirectory);

            int delayLeft = delay;
            int delayInterval = 1000;
            while (delayLeft > delayInterval)
            {
                if (CPH.ObsGetCurrentScene() != scene)
                {
                    CPH.ObsSetBrowserSource(scene, source, "about:blank");
                    CPH.ObsSetGdiText(scene, clipCreditsSource, "");
                    return true;
                }

                CPH.Wait(delayInterval);
                delayLeft -= delayInterval;
            }

            CPH.Wait(delayLeft);
            CPH.LogWarn("Clip finished");
            CPH.ObsSetGdiText(scene, clipCreditsSource, "");
        }

        CPH.ObsSetBrowserSource(scene, source, "about:blank");
        return true;
    }

    private async Task FetchNextVideoUrlAsync(List<Twitch.Common.Models.Api.ClipData> allClips, Random rd, string videoPlayerHtml, string fullNodeServerUrl, string clipCreditsSource, string scene, string workingDirectory)
    {
        if (availableIndices.Count > 0)
        {
            // Select a random clip from the available list
            int randIndex = rd.Next(0, availableIndices.Count);
            int clipIndex = availableIndices[randIndex];
            availableIndices.RemoveAt(randIndex);
            var selectedClip = allClips[clipIndex];

            // Use the embed URL and add "&parent=localhost" to avoid CORS and SOP restrictions
            string embedURL = $"{selectedClip.EmbedUrl}&parent=localhost";
            CPH.LogWarn("Using Embed URL: " + embedURL);

            nextClipInfo = '"' + selectedClip.Title + '"' + " by " + selectedClip.CreatorName;
            nextClipDuration = selectedClip.Duration;  // Capture the duration of the next clip

            // Call the Node.js server to fetch the video's source URL
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"{fullNodeServerUrl}/get-mp4?url={Uri.EscapeDataString(embedURL)}";
                CPH.LogWarn("Requesting video URL from Node.js server: " + requestUrl);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    response.EnsureSuccessStatusCode();

                    string mp4Url = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(mp4Url))
                    {
                        // Use the raw URL & ensure no encoding
                        var fullVideoPlayerHtml = $"{workingDirectory}\\{videoPlayerHtml}";
                        CPH.LogWarn("Video HTML player path : " + fullVideoPlayerHtml);
                        nextVideoUrl = $"file:///{fullVideoPlayerHtml}?{mp4Url}";
                        CPH.LogWarn("Next clip Puppeteer URL: " + nextVideoUrl);
                    }
                    else
                    {
                        CPH.LogError("Failed to retrieve the video URL.");
                        nextVideoUrl = null;
                    }
                }
                catch (Exception ex)
                {
                    CPH.LogError("Error calling Node.js server: " + ex.Message);
                    nextVideoUrl = null;
                }
            }
        }
    }
}
