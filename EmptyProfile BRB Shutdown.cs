using System;
using System.Net.Http;

public class CPHInline
{
    public bool Execute()
    {
        // Import the global variables
        string nodeServerUrl = CPH.GetGlobalVar<string>("NodeServerUrl", true);
        string nodeServerPort = CPH.GetGlobalVar<string>("NodeServerPort", true);

        // Validate the nodeServerPort, default to 3000 if not set or invalid
        if (string.IsNullOrEmpty(nodeServerPort) || !int.TryParse(nodeServerPort, out int port) || port <= 0 || port > 65535)
        {
            CPH.LogWarn($"Invalid or missing NodeServerPort. Defaulting to port 3000.");
            nodeServerPort = "3000";
        }

        // Combine the server URL and port
        string shutdownUrl = $"http://{nodeServerUrl}:{nodeServerPort}/shutdown";

        // Log the information
        CPH.LogInfo($"Sending shutdown request to Node.js server at {shutdownUrl}");

        try
        {
            using (HttpClient client = new HttpClient())
            {
                // Send a GET request to the /shutdown endpoint synchronously
                HttpResponseMessage response = client.GetAsync(shutdownUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    CPH.LogInfo("Node.js server shutdown successfully.");
                    return true;
                }
                else
                {
                    CPH.LogError($"Failed to shutdown Node.js server. Status code: {response.StatusCode}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the HTTP request
            CPH.LogError("Exception occurred while sending shutdown request: " + ex.Message);
            return false;
        }
    }
}
