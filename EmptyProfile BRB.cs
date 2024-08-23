using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        var workingDirectory = CPH.GetGlobalVar<string>("WorkingDirectory", true);
        var nodeJsPath = CPH.GetGlobalVar<string>("NodeJsPath", true);
        var videoPlayerPath = CPH.GetGlobalVar<string>("VideoPlayerPath", true);
        var scriptPath = CPH.GetGlobalVar<string>("ScriptPath", true);

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

        // Set OBS scene to BRB scene
        CPH.ObsSetScene(scene);
        int delayLoop = 0;
        while ((delayLoop < 25) && (CPH.ObsGetCurrentScene() != scene))
        {
            CPH.Wait(250);
            delayLoop++;
        }

        Random rd = new Random();

        // Start fetching the first video URL asynchronously
        Task fetchNextVideoTask = FetchNextVideoUrlAsync(allClips, rd, videoPlayerPath, scriptPath, workingDirectory, nodeJsPath, clipCreditsSource, scene);

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
            fetchNextVideoTask = FetchNextVideoUrlAsync(allClips, rd, videoPlayerPath, scriptPath, workingDirectory, nodeJsPath, clipCreditsSource, scene);

            delay += 800;  // Extra delay for transition and buffer
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

    private async Task FetchNextVideoUrlAsync(List<Twitch.Common.Models.Api.ClipData> allClips, Random rd, string videoPlayerPath, string scriptPath, string workingDirectory, string nodeJsPath, string clipCreditsSource, string scene)
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

            // Start Puppeteer to fetch the video's source URL
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = nodeJsPath,  // Node.js path
                Arguments = $"{scriptPath} \"{embedURL}\"",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    CPH.LogError("Puppeteer Error: " + error);
                    nextVideoUrl = null;
                }
                else
                {
                    CPH.LogWarn("Puppeteer Output: " + output);

                    // Get the link from output
                    string[] outputLines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    string directVideoUrl = outputLines.Length > 0 ? outputLines[0] : null;

                    if (!string.IsNullOrEmpty(directVideoUrl))
                    {
                        // Use the raw URL & ensure no encoding
                        nextVideoUrl = $"file:///{videoPlayerPath}?{directVideoUrl}";
                        CPH.LogWarn("Next clip Puppeteer URL: " + nextVideoUrl);
                    }
                    else
                    {
                        CPH.LogError("Failed to extract Direct Video URL.");
                        nextVideoUrl = null;
                    }
                }
            }
        }
    }
}
