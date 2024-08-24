
# BRB Clip Player for Twitch Streams

This project provides an automated solution for displaying Twitch clips in your "Be Right Back" (BRB) scene using Streamer.bot, a Node.js server, Express and Puppeteer. It's based on the original BRB player concept by VRFlad, optimized for handling edited Twitch clips effectively.

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

Streamer.bot will start the server on stream start and shut it down on stream end. For this reason, it is required that Streamer.bot is started before the stream.
- If you started Streamer.bot after the stream or killed it before the end of the stream, you can easily turn on (BRB Init) and off (BRB Shutdown) the Node.js server by right-clicking the triggers and select "test trigger".

## Installation

### Prerequisites
- [Node.js](https://nodejs.org/) installed on your machine.
- [Puppeteer](https://github.com/puppeteer/puppeteer) installed in your project.
- [Express](https://expressjs.com/) installed in your project 

### Steps
0. **Create a project directory**
   - On your computer, create a folder where all the files and Node.js modules will be placed. 
   - This folder's path must be set to the `WorkingDirectory` variable in Streamer.bot 

1. **Install Node.js, Express and Puppeteer:**
   - Install Node.js from the official website.
   - Run `npm install puppeteer` in your project directory to install Puppeteer 
   - Run `npm install  express` in your project directory to install  Express.

2. **Set Up the Project Directory:**
   - Place the `server.js` script in your project folder.
   - Place the `video_player.html` script in your project folder.
   - The `VideoPlayerHtml` and `JavascriptServer` Streamerbot variables expect the respective `.html` and `.js` files to be located in the folder set up in the `WorkingDirectory` variable.

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
In Streamer.bot, go to the EmptyProfile BRB Config action and set up the following variables:
   - **BRBBrowserSource:** The name of your BRB browser source in OBS.
   - **BRBScene:** The name of your BRB scene in OBS.
   - **ClipCreditsSource:** The name of your text source for clip credits.
   - **WorkingDirectory:** The directory path where your project files are located.
   - **NodeJsPath:** The full path to your Node.js installation (`node.exe`).
   - **VideoPlayerHtml:** The name of your HTML video player file (`video_player.html` in this project). 
   - **JavascriptServer:** The name of your JavaScript server file (`server.js` in this project).
   - **NodeServerUrl:** The URL for your Node.js server (default is `localhost`).
   - **NodeServerPort:** The port for your Node.js server (default is `3000`).

## Explanation of Changes

The solution has been modified to handle different types of Twitch clip URLs effectively, ensuring that the correct portion of the clip is played. This requires fetching the actual video URL with a token and other parameters using Puppeteer, bypassing the limitations of embedding directly through the browser. Additionally, a Node.js server is now launched in the background to handle clip processing more efficiently.

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

1. **Streamer.bot:** Launches the Node.js server on stream start and grabs a list of the user's clips. When the BRB is triggered (either by the !brb command or the OBS source change), Streamerbot sends an embedded clip URL to the server.
2. **Server** Extracts the source video URL out of the embed video URL and returns it to Streamer.bot.
3. **Streamer.bot:** Sets the OBS scene containing the player to the correct URL.
4. **Player:** Plays whatever video is specified after the "?" in its URL.
5. **Streamer.bot:** Shuts down the server on stream end.

Additionally, a "credit" option is included to easily display the title and author of the clip. The script also ensures that the same clip isn't played twice until all clips have been broadcasted.

## Commands

- `!brb`: Switches to your BRB scene and starts rolling clips (command available to mods by default).
- Manually changing your OBS source to your BRB scene will also trigger the script.
- Start stream trigger: Starts the Node.js server.
- Stop stream trigger: Stops the Node.js server.
