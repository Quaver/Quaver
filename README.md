# Quaver [![CodeFactor](https://www.codefactor.io/repository/github/swan/quaver/badge)](https://www.codefactor.io/repository/github/swan/quaver) [![Discord](https://discordapp.com/api/guilds/354206121386573824/widget.png?style=shield)](https://discord.gg/nJa8VFr)
Quaver is a modern cross-platform vertically scrolling rhythm game available for Windows, OS X & Linux.

# Status
Quaver is still under development and should not be used for gameplay, and is only intended for developers only. While we do provide builds for users, expect there to be bugs and things not working. Any feature requests should be opened in the [issues](https://github.com/Swan/Quaver/issues).

# Requirements
* Unity 5.0+
* .NET 2.0+

# Contributing 
Currently we are not looking for any community-based contributions. As this repository does not contain a license, all code in the repository is copyrighted. If you are eager to contribute however, it is best to contact us directly. Any pull requests made will be closed. Any feedback in terms of our code strucuture, or an issue with a library we are using, feel free to [open an issue](https://github.com/Swan/Quaver/issues).

# Documentation
To make life a bit easier for everyone, here's a bit of documentation for some of the certain aspects of Quaver that should be known about.

## .Qua Files ##

.qua is our extension for beatmaps. We have made it very easy to comprehend and modify. Take a look at the table below, which describes each individual aspect of a qua file. Please note that any of these fields are subject to change at anytime.

### General ###

This section contains general information about the beatmap.

| Field | Description | Data Type | Required | Example |
| --- | --- | --- | --- | --- |
| **AudioFile** | The **.ogg** file that the beatmap will play | String | Yes | audio.ogg |
| **AudioLeadIn** | The amount of milliseconds before the audio file starts playing | Integer | Yes | 0
| **SongPreviewTime** | The offset/amount of milliseconds in the song which we be played as a preview during song select | Integer | Yes | 9247
| **BackgroundFile** | The **.jpg** or **.png** file that will be used as the beatmap's background | String | No | "nice_cats.jpg"

### Metadata ###

This section contains all of the metadata for the beatmap

| Field | Description | Data Type | Required | Example |
| --- | --- | --- | --- | --- |
| **Title** | The song title | String | Yes | Title
| **TitleUnicode** | The song title with any Unicode characters | String | Yes | Title (Unicode)
| **Artist** | The song's artist | String | Yes | Camellia
| **ArtistUnicode** | The song's artist with any unicode characters | String | Yes | Camellia (Unciode)
| **Source** | The source of the song (album, mixtape, etc) | String | No | The Blueprint
| **Tags** | Tags for the song, usually its genre or relating details | String | No | dnb dubstep vibro
| **Creator** | The creator of the beatmap | String | No | arpia97
| **DifficultyName** | The custom difficulty name of the beatmap | String | Yes | GRAVITY
| **MapID** | The beatmap id of the map (Not implemented) | Integer | Yes | -1 (Default)
| **MapSetID** | The beatmap set id ofthe map (Not implemented) | Integer | Yes | -1 (Default)

### Difficulty ###

This section contains everything that has to do with how difficult the map is in terms of accuracy and hitpoints.

| Field | Description | Data Type | Required | Example |
| --- | --- | --- | --- | --- |
| **HPDrain** | The amount of drain ranging from 0-10 in which hitpoints drops when missing | Float | Yes | 7.7
| **AccuracyStrain** | The higher it is, the tighter the hit window is. Ranges from 0-10 | Float | Yes | 8

### Timing ###

This section contains information about how we handle timing points. This is different than the previous sections considering each timing section has multiple parts. 

A timing section is denoted by two floats separated by a pipe. (Start Time|BPM)

| Field | Description | Data Type | Required | Example |
| --- | --- | --- | --- | --- |
| **Start Time** | The offset in the song where the timing point starts | Float | Yes | 17
| **BPM** | The beats per minute of that individual timing point | Float | Yes | 175

The timing point from the table above would be noted as ``17|175`` in a .qua file.

### SV ###

This section contains information about how we handle Slider Velocities. A slider velocity is essentially the speed at which objects fall. You can customize the rate to be faster or slower here. 

A SV is denoted by two floats and an integer separated by pipes. (Start Time|Multiplier|BPM).

| Field | Description | Data Type | Required | Example |
| --- | --- | --- | --- | --- |
| **Start Time** | The offset in the song in which the SV will begin to occur | Float | Yes | 2812
| **Multiplier** | The rate at which objects fall. | Float | Yes | 0.5 (This would be half speed)
| **Volume** | The volume of the hitsounds during this SV (Ranges from 0-100) | Float | Yes | 55

In this case, the SV would be noted as ``2812|0.5|55`` in a .qua file

### HitObjects ###

This section of the .qua file is where all the notes or Hit Objects are. There are two types of hit objects - a `note` and a `Long Note`, but it should be relatively easy to understand how they work. 

A HitObject is denoted by three integer values separated by pipes (|).

| Field | Description | Data Type | Required | Example |
| --- | --- | --- | --- | --- |
| **Start Time** | The offset at which the hit object begins | Integer | Yes | 7182 (This would be 7.182 seconds into the song) |
| **Key Lane** | The lane in which the note will fall (From 1-4) | Integer | Yes | 3 |
| **End Time** | The offset at which denotes the end time of a note. **Important:** If this is greater than 0, the note would be considered as a Long Note. | Integer | Yes | 9182

In this case, the HitObject would be noted as such: ``7182|3|9182``

All of this information combined into a file makes up a .qua file which is able to be played. Please note that files must be in this exact format, if not, then the beatmap might not be able to be played. For more information, please see an [example file](https://github.com/Swan/Quaver/blob/master/Test/Qua/some.qua).

## Configuration (quaver.cfg) File ##

Quaver.cfg is essentially your configuration file for the game. You shouldn't really touch it unless you know what you are doing. A configuration file is generated on game load if you do not already have one, or if you have one that isn't valid. Below explains all the fields involved in this file.

| Field | Description | Data Type | Example |
| --- | --- | --- | --- | 
| **SongDirectory** | The directory at which your songs will be loaded from | String | C:/Quaver/Songs/ |
| **SkinsDirectory** | The directory at which your skins will be loaded | String | C:/Quaver/Skins/ |
| **ScreenshotsDirectory** | The directory at which your screenshots will be saved | String | C:/Quaver/Screenshots |
| **ReplaysDirectory** | The directory at which all exported replays will be saved to | String | C:/Quaver/Replays |
| **LogsDirectory** | The directory at which all error, network, gamelogs will be stored | String | C:/Quaver/Logs |
| **VolumeGlobal** | The master volume at which the game plays at ranging from 0-100 | Byte | 75 |
| **VolumeEffect** | The volume at which all effects in the game play at ranging from 0-100 | Byte | 25 |
| **VolumeMusic** | The volume at which all music in the game plays at ranging from 0-100 | Byte | 99 |
| **BackgroundDim** | The preset dim at which backgrounds will be at ranging from 0-100 | Byte | 100 (All the way dimmed) |
| **WindowHeight** | The saved window height the game will start at | Integer | 1080 |
| **WindowWidth** | The saved window width the game will start at | Integer | 1920 |
| **WindowFullScreen** | Is the game in fullscreen? | Bool | False |
| **WindowLetterbox** | Is the game letterboxed? | Bool | True |
| **CustomFrameLimit** | The FPS limit of the game | Short | 240 (Default) |
| **FPSCounter** | Will the game display the FPS Counter? | Bool | True |
| **FrameTimeDisplay** | Will the game display a graph of Frame Times? (Not Implemented) | Bool | False |
| **Language** | The language of the game (Not Implemented) | String | en |
| **QuaverVersion** | The version of the game | String | db.0.0.1 |
| **QuaverBuildHash** | The build hash of the game | String | Not Implemented |
| **ScrollSpeed** | The scroll speed at which objects will fall down | Byte | 32 |
| **ScrollSpeedBPMScale** | Will the scroll speed be scaled by the BPM? | Bool | False |
| **DownScroll** | If true, the notes fall downwards, if false they fall upward | Bool | True |
| **GlobalOffset** | The offset at which notes will start playing in accordance to the song | Byte | 0 (Default) |
| **LeaderboardVisible** | Do you want to show a scores leaderboard? (Not Implemented) | Bool | False |
| **Skin** | The folder name of the loaded skin | String | Default |
| **KeyLaneMania1** | The key pressed for lane 1 | String | D |
| **KeyLaneMania2** | The key pressed for lane 2 | String | F |
| **KeyLaneMania3** | The key pressed for lane 3 | String | J |
| **KeyLaneMania4** | The key pressed for lane 4 | String | K |
| **KeyScreenshot** | The key pressed to take a screenshot | String | F12 |
| **KeyQuickRetry** | The key pressed to quickly retry a map | String | BackQuote |
| **KeyIncreaseScrollSpeed** | The key pressed to increase the scroll speed by 1 | String | F5 |
| **KeyDecreaseScrollSpeed** | The key pressed to decerease the scroll speed by 1 | String | F4 |
| **KeyPause** | The key pressed to pause the game | String | ESC |
| **KeyVolumeUp** | The key pressed to turn the volume of the game up | String | UpArrow |
| **KeyVolumeDown** | The key pressed to turn the volume of the game down | String | DownArrow |
| **TimingBars** | Do you want to show timing bars while playing? | Bool | True |

# Copyright
All code in this repository is copyrighted by the repository organization: [Quaver](https://github.com/Quaver) and its owners and does not contain an open-source license. Without explicit permission, you are not allowed to: use, copy, distribute, or modify any code without being at risk of take-downs, shake-downs, or litigation. It is best to download the client from the official source or Github release rather than compiling, modifying, or distributing your own. 
