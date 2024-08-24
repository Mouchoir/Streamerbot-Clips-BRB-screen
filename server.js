const express = require('express');
const puppeteer = require('puppeteer');

const app = express();
const port = 3000;

let browser;

// Helper function to log with a timestamp
function logWithTimestamp(message) {
    const timestamp = new Date().toISOString();
    console.log(`[${timestamp}] ${message}`);
}

// Launch Puppeteer when the server starts
(async () => {
    try {
        browser = await puppeteer.launch({ headless: true });
        logWithTimestamp('Puppeteer launched successfully');
    } catch (error) {
        logWithTimestamp(`Error launching Puppeteer: ${error}`);
        process.exit(1); // Exit the server if Puppeteer fails to launch
    }
})();

// Main route to handle GET requests with the URL as a parameter
app.get('/get-mp4', async (req, res) => {
    const embedUrl = req.query.url;

    logWithTimestamp(`Received request for /get-mp4 with URL: ${embedUrl}`);

    if (!embedUrl) {
        logWithTimestamp('Missing URL parameter');
        return res.status(400).send('Missing URL parameter');
    }

    let found = false;

    try {
        logWithTimestamp('Creating page');
        const page = await browser.newPage();
        logWithTimestamp(`New page created for URL: ${embedUrl}`);

        // Promise to capture the .mp4 URL
        const mp4Promise = new Promise((resolve) => {
            logWithTimestamp('Promise started');
            page.on('response', async (response) => {
                const url = response.url();
                if (url.includes('.mp4') && !found) {
                    found = true;
                    logWithTimestamp(`.mp4 URL found: ${url}`);
                    resolve(url);
                }
            });
        });

        await page.goto(embedUrl);
        logWithTimestamp(`Navigating to URL: ${embedUrl}`);

        const mp4Url = await mp4Promise;

        // Close the page after use
        await page.close();
        logWithTimestamp('Page closed');

        if (found) {
            res.send(mp4Url);
        } else {
            logWithTimestamp('No .mp4 URL found.');
            res.status(404).send('No .mp4 URL found.');
        }
    } catch (error) {
        logWithTimestamp(`Error executing Puppeteer script: ${error}`);
        res.status(500).send('Error executing Puppeteer script');
    }
});

// Route to shutdown the server
app.get('/shutdown', async (req, res) => {
    logWithTimestamp('Received request for /shutdown');
    res.send('Server is shutting down...');

    try {
        // Close Puppeteer and shut down the server
        await browser.close();
        logWithTimestamp('Puppeteer closed successfully');
    } catch (error) {
        logWithTimestamp(`Error closing Puppeteer: ${error}`);
    }

    process.exit(0);
});

// Start the server
app.listen(port, () => {
    logWithTimestamp(`Server is listening on http://localhost:${port}`);
});
