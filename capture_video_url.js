const puppeteer = require('puppeteer');

(async () => {
    const browser = await puppeteer.launch({ headless: true });
    const page = await browser.newPage();

    const embedUrl = process.argv[2];

    if (!embedUrl) {
        console.error("The embed URL is required as an argument.");
        process.exit(1);
    }

    let found = false;

    // Promise to capture the .mp4 URL
    const mp4Promise = new Promise((resolve) => {
        page.on('response', async (response) => {
            const url = response.url();
            if (url.includes('.mp4') && !found) {
                found = true;
                console.log(url);
                resolve();
            }
        });
    });

    await page.goto(embedUrl);
    await mp4Promise;

    if (!found) {
        console.error("No .mp4 URL found.");
    }

    await browser.close();
    process.exit(found ? 0 : 1);
})();
// https://www.twitch.com/emptyprofile