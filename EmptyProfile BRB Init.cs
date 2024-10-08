using System;
using System.Diagnostics;
using System.IO;

public class CPHInline
{
    public bool Execute()
    {
        // Import the global variables
        string javascriptServer = CPH.GetGlobalVar<string>("JavascriptServer", true);
        string workingDirectory = CPH.GetGlobalVar<string>("WorkingDirectory", true);
        string nodeJsPath = CPH.GetGlobalVar<string>("NodeJsPath", true);
        string nodeServerPort = CPH.GetGlobalVar<string>("NodeServerPort", true);

        // Loading all clips to improve loading time
        string userName = args["targetUser"].ToString();
        var allClips = CPH.GetClipsForUser(userName);

        if (allClips.Count == 0)
        {
            CPH.SendMessage("You do not have clips to play!");
            return false;
        }

        CPH.SetGlobalVar("BrbAllClips", allClips);

        // Validate the nodeServerPort, default to 3000 if not set or invalid
        if (string.IsNullOrEmpty(nodeServerPort) || !int.TryParse(nodeServerPort, out int port) || port <= 0 || port > 65535)
        {
            CPH.LogWarn($"Invalid or missing NodeServerPort. Defaulting to port 3000.");
            nodeServerPort = "3000";
        }

        // Combine the working directory with the script file name
        string javascriptServerPath = Path.Combine(workingDirectory, javascriptServer);

        // Log the information
        CPH.LogInfo($"Starting Node.js server using script: {javascriptServerPath} on port {nodeServerPort}");

        // Configure the process start info
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = nodeJsPath,
            Arguments = $"\"{javascriptServerPath}\"",  // Quote the script path to handle spaces
            WorkingDirectory = workingDirectory,  // Set the working directory
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.EnvironmentVariables["PORT"] = nodeServerPort;  // Set the PORT environment variable

        try
        {
            // Start the Node.js server process
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Read the output (optional, for debugging or logging)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                CPH.LogDebug("Node.js Server Output: " + output);

                if (!string.IsNullOrEmpty(error))
                {
                    CPH.LogError("Node.js Server Error: " + error);
                    return false;
                }

                // Optionally, you could wait for the process to exit, but typically you want the server to keep running
                // process.WaitForExit();

                CPH.LogDebug("Node.js server started successfully.");
            }

            return true;
        }
        catch (Exception ex)
        {
            CPH.LogError("Failed to start Node.js server: " + ex.Message);
            return false;
        }
    }
}
