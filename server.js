const express = require('express');
const puppeteer = require('puppeteer');

const app = express();
const port = 3000;

let browser;

// Launch Puppeteer when the server starts
(async () => {
    try {
        browser = await puppeteer.launch({ headless: true });
        console.log('Puppeteer launched successfully');
    } catch (error) {
        console.error('Error launching Puppeteer:', error);
        process.exit(1); // Exit the server if Puppeteer fails to launch
    }
})();

// Main route to handle GET requests with the URL as a parameter
app.get('/get-mp4', async (req, res) => {
    const embedUrl = req.query.url;

    // console.log(`Received request for /get-mp4 with URL: ${embedUrl}`);

    if (!embedUrl) {
        // console.log('Missing URL parameter');
        return res.status(400).send('Missing URL parameter');
    }

    let found = false;

    try {
        // Create a new page for each request
        const page = await browser.newPage();
        // console.log(`New page created for URL: ${embedUrl}`);

        // Promise to capture the .mp4 URL
        const mp4Promise = new Promise((resolve) => {
            // console.log(`Promise started`);
            page.on('response', async (response) => {
                // console.log(`Page response received`);
                const url = response.url();
                if (url.includes('.mp4') && !found) {
                    found = true;
                    // console.log(`.mp4 URL found: ${url}`);
                    resolve(url);
                }
            });
        });

        await page.goto(embedUrl);
        // console.log(`Navigating to URL: ${embedUrl}`);

        const mp4Url = await mp4Promise;

        // Close the page after use
        await page.close();
        // console.log('Page closed');

        if (found) {
            res.send(mp4Url);
        } else {
            console.log('No .mp4 URL found.');
            res.status(404).send('No .mp4 URL found.');
        }
    } catch (error) {
        console.error('Error executing Puppeteer script:', error);
        res.status(500).send('Error executing Puppeteer script');
    }
});

// Route to shutdown the server
app.get('/shutdown', async (req, res) => {
    console.log('Received request for /shutdown');
    res.send('Server is shutting down...');

    try {
        // Close Puppeteer and shut down the server
        await browser.close();
        console.log('Puppeteer closed successfully');
    } catch (error) {
        console.error('Error closing Puppeteer:', error);
    }

    process.exit(0);
});

// Start the server
app.listen(port, () => {
    console.log(`Server is listening on http://localhost:${port}`);
});
