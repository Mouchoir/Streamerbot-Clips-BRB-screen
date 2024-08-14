
# BRB Clip Player for Twitch Streams

This project provides an automated solution for displaying Twitch clips in your "Be Right Back" (BRB) scene using Streamer.bot, Node.js, and Puppeteer. It's based on the original BRB player concept by VRFlad, optimized for handling edited Twitch clips effectively.

## Index
- [Introduction](#introduction)
- [Installation](#installation)
- [OBS Setup](#obs-setup)
- [Streamer.bot Setup](#streamerbot-setup)
- [Explanation of Changes](#explanation-of-changes)
- [Types of Twitch Clip URLs](#types-of-twitch-clip-urls)
- [How It Works](#how-it-works)
- [Commands](#commands)

## Introduction

This project is based on the BRB player by [VRFlad](https://vrflad.com/) and has been optimized to correctly display edited Twitch clips. It fetches all your Twitch clips and plays them in your BRB scene during breaks in your stream.

## Installation

### Prerequisites
- [Node.js](https://nodejs.org/) installed on your machine.
- [Puppeteer](https://github.com/puppeteer/puppeteer) installed in your project.

### Steps
1. **Install Node.js and Puppeteer:**
   - Install Node.js from the official website.
   - Run `npm install puppeteer` in your project directory to install Puppeteer.

2. **Set Up the Project Directory:**
   - Place the `capture_video_url.js` Puppeteer script in your project folder.
   - Ensure all related files (e.g., the video player HTML file) are in the same directory for easier management.

3. **Streamer.bot Code:**
   - Import the provided Streamer.bot `.sb` file or copy and paste the code directly into Streamer.bot.

## OBS Setup

1. **Create a Browser Source:**
   - Set the browser source URL to `about:blank`. This will be the window where your Twitch clips are displayed.
   
2. **Create a Text GDI+ Source:**
   - Optional: Use this source for displaying the clip's title and creator as credits.

## Streamer.bot Setup

1. **Import the Script:**
   - Import the provided `.sb` file or use the import code below to bring the setup into Streamer.bot. 
   - Import code:
   
```
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
```

2. **Configure Subactions variables:**
   - **BRBBrowserSource:** The name of your BRB browser source in OBS.
   - **BRBScene:** The name of your BRB scene in OBS.
   - **ClipCreditsSource:** The name of your text source for clip credits.
   - **WorkingDirectory:** The directory path where your Puppeteer scripts are located.
   - **NodeJsPath:** The full path to your Node.js installation (`node.exe`).
   - **VideoPlayerPath:** The path to your video player HTML file.
   - **ScriptPath:** The path to the `capture_video_url.js` script.

## Explanation of Changes

The solution has been modified to handle different types of Twitch clip URLs effectively, ensuring that the correct portion of the clip is played. This requires fetching the actual video URL with a token and other parameters using Puppeteer, bypassing the limitations of embedding directly through the browser.

## Types of Twitch Clip URLs

1. **Twitch Clip Page URL:**
   - **Example:** `https://clips.twitch.tv/IronicVivaciousMomTTours-s-p4PwURdCld4ilw`
   - **How to Obtain:** This is the URL you get directly from Twitch when you clip a portion of a stream.
   - **Problem:** This URL leads to the full Twitch clip page, which includes the Twitch player and additional UI elements. It does not directly provide a way to embed only the edited portion of the video as a raw `.mp4` file.

2. **Direct Media Asset URL:**
   - **Example:** `https://clips-media-assets2.twitch.tv/WPUMpbPqeD8FEXQpXeIpcA/40910109621-offset-9052.mp4`
   - **How to Obtain:** This URL can sometimes be derived from inspecting the network requests made when the clip is played or using a guessed format based on other media assets.
   - **Problem:** The video obtained through this URL often includes the full unedited clip, which may not reflect the precise start and end points that the clip creator intended. This means you might get more content than you want if the clip was trimmed.

3. **Embedded Twitch Player URL:**
   - **Example:** `https://clips.twitch.tv/embed?clip=IronicVivaciousMomTTours-s-p4PwURdCld4ilw&parent=localhost`
   - **How to Obtain:** This is a modified version of the Twitch Clip Page URL, designed for embedding in iframes.
   - **Problem:** This URL embeds the Twitch player within an iframe, which includes all Twitch branding and controls, not just the raw video. It does, however, correctly play only the edited portion of the clip.

4. **Direct Video File URL (via Network Inspection):**
   - **Example:** `https://production.assets.clips.twitchcdn.net/v2/media/IronicVivaciousMomTTours-s-p4PwURdCld4ilw/0e0ad629-8ef5-4bd5-a50e-b4c49f851801/video.mp4?sig={sig}8&token={token}`
   - **How to Obtain:** This URL can be found by inspecting network requests in the browser when the clip is played in the Twitch player. The URL is usually tokenized, meaning it has a short expiration time.
   - **Problem:** The main issue here is that the URL contains a token and `sig` parameter that expires. Without these, the URL won’t work. Additionally, you need to manually inspect the network to get this URL, which isn't straightforward.

## How It Works

1. **Streamer.bot:** Fetches the clips URL and feeds it to Puppeteer.
2. **Puppeteer:** Extracts the source video URL out of the embed video URL and returns it to Streamer.bot.
3. **Streamer.bot:** Sets the OBS scene containing the player to the correct URL.
4. **Player:** Plays whatever video is specified after the "?" in its URL.

Additionally, a "credit" option is included to easily display the title and author of the clip. The script also ensures that the same clip isn't played twice until all clips have been broadcasted.

## Commands

- `!brb`: Switches to your BRB scene and starts rolling clips (command available to mods by default).
- Manually changing your OBS source to your BRB scene will also trigger the script.
