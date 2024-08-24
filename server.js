const express = require('express');
const puppeteer = require('puppeteer');

const app = express();

// Use the PORT environment variable, or default to port 3000 if not set
let port = process.env.PORT;

// Validate the port number
if (isNaN(port) || port <= 0 || port > 65535) {
    logWithTimestamp(`Invalid or missing port number. Defaulting to port 3000.`);
    port = 3000;
}

// Helper function to log with a timestamp
function logWithTimestamp(message) {
    const timestamp = new Date().toISOString();
    console.log(`[${timestamp}] ${message}`);
}

// Launch Puppeteer when the server starts
let browser;
(async () => {
    try {
        browser = await puppeteer.launch({
            headless: true,
            args: ['--no-sandbox', '--disable-setuid-sandbox'],
        });
        logWithTimestamp('Puppeteer launched successfully');
    } catch (error) {
        logWithTimestamp(`Error launching Puppeteer: ${error}`);
        process.exit(1); // Exit the server if Puppeteer fails to launch
    }
})();

// Main route to handle GET requests with the URL as a parameter
app.get('/get-mp4', async (req, res) => {
    const embedUrl = req.query.url;

    if (!embedUrl) {
        logWithTimestamp('Missing URL parameter');
        return res.status(400).send('Missing URL parameter');
    }

    let found = false;

    try {
        const page = await browser.newPage();

        // Block unnecessary resources
        await page.setRequestInterception(true);
        page.on('request', (request) => {
            const resourceType = request.resourceType();
            if (['image', 'stylesheet', 'font', 'media'].includes(resourceType)) {
                request.abort(); // Abort loading images, stylesheets, fonts, and media
            } else {
                request.continue();
            }
        });

        // Intercept network requests to identify the .mp4 URL as early as possible
        const mp4Promise = new Promise((resolve, reject) => {
            const onResponse = async (response) => {
                const url = response.url();
                if (url.includes('.mp4') && !found) {
                    found = true;
                    page.off('response', onResponse); // Remove the event listener
                    resolve(url);
                }
            };
            page.on('response', onResponse);
        });

        await page.goto(embedUrl, { waitUntil: 'domcontentloaded' });

        const mp4Url = await mp4Promise;

        // Close the page after use
        await page.close();

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
