# Quaver 

[![Build Status](https://travis-ci.com/Quaver/Quaver.svg?branch=develop)](https://travis-ci.com/Quaver/Quaver) [![CodeFactor](https://www.codefactor.io/repository/github/swan/quaver/badge)](https://www.codefactor.io/repository/github/swan/quaver) [![license](https://img.shields.io/badge/license-Mozilla%20Public%20License%202.0-blue)](https://github.com/Quaver/Quaver/blob/develop/LICENSE) [![Discord](https://discordapp.com/api/guilds/354206121386573824/widget.png?style=shield)](https://discord.gg/nJa8VFr) <a href="https://twitter.com/intent/follow?screen_name=QuaverGame">
        <img src="https://img.shields.io/twitter/follow/QuaverGame?style=social&logo=twitter"
            alt="follow on Twitter"></a> [![stargazers](https://img.shields.io/github/stars/Quaver/Quaver?style=social)](#)

<p align="center"> 
  <img src="https://i.imgur.com/AohWq5l.png">
</p>

| [![steam](https://i.imgur.com/rR4p9mW.png)](https://store.steampowered.com/app/980610/Quaver/) | [![twitter](https://i.imgur.com/Kd0SXlh.png)](https://twitter.com/QuaverGame) | [![discord](https://i.imgur.com/YrBmbEf.png)](https://discord.gg/nJa8VFr) | [![blog](https://i.imgur.com/o0fq6MA.png)](https://blog.quavergame.com) | [![website](https://i.imgur.com/svZCnuI.png)](https://quavergame.com) | 
| --- | --- | --- | --- | --- |
| **Steam Store Page** | **Follow Us On Twitter** | **Join The Discord** | **Read The Developer Blog** | **Visit The Website** |

## About 

**Quaver** is a community-driven and open-source competitve vertical scrolling rhythm game with two game modes and online leaderboards. It also features an in-game editor and a flexible multiplayer mode.

It is also being officially released on [Steam](https://store.steampowered.com/app/980610/Quaver/) for Windows, Mac, and Linux - making it one of the most accessible community-driven rhythm games to date.

This project is currently under heavy development and is in an alpha state, however it is extremely playable as a standalone game. Quaver still has a ton of development ahead of it and will continue to get better as time goes on with new features and bug fixes being added in each day.

Occasionally we open registrations where you can join the Steam alpha and get exclusive access to all the features Quaver has to offer including online rankings, multiplayer, in-game chat, map downloading/uploading, and so much more. Registrations are **currently closed,** however you can still enjoy a limited version of Quaver by compiling the `develop` branch or by downloading one of the [GitHub Releases](https://github.com/Quaver/Quaver/releases).

## Features

Although there are many games like this, **Quaver** offers many unique features and expands on the ideas of its predecessors that make it much different. It is also heavily inspired by a variety of our favorite rhythm games.

* **Two Game Modes** - Play the game with 4 keys or challenge yourself with 7. Each game mode has separate global and country leaderboards to compete on.

* **Global & Country Leaderboards** - Compete with your country and the world for the highest scores and aim to be #1.

* **Ranked & Casual Multiplayer** - Challenge up to 16 players in casual or competitive-oriented multiplayer battles. Multiplayer includes three game modes including Free-For-All, Teams, and Battle Royale

* **Map Editor** - Create your own maps to any of your favorite songs. Upload and share them with the world, and submit them for official ranking.

* **Advanced Skinning System** - Completely customize your gameplay experience with the ability to create skins. Export and share your skins with friends or upload them to the Steam workshop.

* **Replays** - Go back in time by watching your previous scores. Watch replays from other players around the world, or export your own and share them with your rivals.

* **Over 10+ Game Modifiers** - Switch up the way you play by activating in-game modifiers. Customize the speed of the song, get rid of all the long notes, or even randomize the entire map!

* **Steam Achievements** - Become a master of **Quaver** by completing challenges and earning achievements!

* **Play Maps From Other Games** - Coming from a different game and miss all of your favorite maps? Quaver supports both .osz and .sm files out of the box - with support for more games to be added in the future!

* **Join a Growing Community** - This game is 100% community-driven and built from the ground up with players' feedback in mind.

* **Anyone Can Contribute** - Submit feedback, discuss with the developers, and see your ideas come to life in-game.

## Building & Running

Getting started with **Quaver** development is extremely easy.

* Install the [.NET Core 2.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.1)
* Clone the Quaver repository and its submodules `git clone --recurse-submodules https://github.com/Quaver/Quaver`
* **Have Steam open and running**
* Build & run Quaver with `dotnet run --project Quaver`

## Contributing 

The best place to begin contributing to Quaver is through our [Discord server](https://discord.gg/nJa8VFr), where all of our developers, community, and testers are located.

Any contributions can be made through opening a pull request. When contributing however, please keep in mind that there should only be one branch/pull request per feature. This means if your branch is titled `my-new-feature`, you shouldn't have any unrelated commits, and the same system is applied to pull requests. Please make sure to keep your pull requests short and concise.

If you are wanting to develop a feature with the goal of having it being in the Steam release, open up an issue first, so we can discuss if it is in the scope of what we are trying to accomplish.

When contributing, please remember to follow our [code style](https://github.com/Quaver/Quaver/blob/master/CODESTYLE.md), so the codebase is consistent across the board. If you have any issues with our approaches to structuring/styling our code, feel free to bring this up.

## LICENSE

The Quaver game client is split up into submodules which are subject to their own individual licensing. Please see each submodule to view their respective license(s).

The code in this repository is released and licensed under the [Mozilla Public License 2.0](https://github.com/Quaver/Quaver/blob/develop/LICENSE). Please see the [LICENSE](https://github.com/Quaver/Quaver/blob/develop/LICENSE) file for more information. In short, if you are making any modifications to this software, you **must** disclose the source code of the modified version of the file(s), and include the original copyright notice.

Please be aware that all game assets are released and covered by a separate license. This should be noted when using this software to create derivatives for commercial purposes. Please see the [Quaver.Resources](https://github.com/Quaver/Quaver.Resources) repository for further information regarding licensing.
