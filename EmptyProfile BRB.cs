using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

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


        // Get the target user from arguments
        string userName = args["targetUser"].ToString();
        var allClips = CPH.GetClipsForUser(userName);

        if (allClips.Count == 0)
        {
            CPH.SendMessage("You do not have clips to play!");
            return false;
        }

        // Initialize available indices if empty
        if (availableIndices == null || availableIndices.Count <= 0)
        {
            availableIndices = new List<int>();
            for (int i = 0; i < allClips.Count; i++)
            {
                availableIndices.Add(i);
                CPH.LogWarn("All Clips Title: " + allClips[i].Title + " | Index: " + i);
            }
        }

        Random rd = new Random();

        string fullNodeServerUrl = $"http://{nodeServerUrl}:{nodeServerPort}";
        CPH.LogWarn("Server URL " + fullNodeServerUrl);

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

            // Set delay based on the clip's duration + 250ms for safety
            int delay = (int)(nextClipDuration * 1000) + 250;

            // Fetch the next video URL while the current video is playing
            fetchNextVideoTask = FetchNextVideoUrlAsync(allClips, rd, videoPlayerHtml, fullNodeServerUrl, clipCreditsSource, scene, workingDirectory);

            int delayLeft = delay;
            while (delayLeft > 1000)
            {
                if (CPH.ObsGetCurrentScene() != scene)
                {
                    CPH.ObsSetBrowserSource(scene, source, "about:blank");
                    CPH.ObsSetGdiText(scene, clipCreditsSource, "");
                    return true;
                }

                CPH.Wait(1000);
                delayLeft -= 1000;
            }

            CPH.Wait(delayLeft);
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
