# Quaver [![CodeFactor](https://www.codefactor.io/repository/github/swan/quaver/badge)](https://www.codefactor.io/repository/github/swan/quaver) [![Discord](https://discordapp.com/api/guilds/354206121386573824/widget.png?style=shield)](https://discord.gg/nJa8VFr)
Quaver is a modern cross-platform vertically scrolling rhythm game available for Windows, OS X & Linux. 

### Table of Contents ###
* [Status](https://github.com/Swan/Quaver#status)
* [Requirements](https://github.com/Swan/Quaver#requirements)
* [Submodules](https://github.com/Swan/Quaver#submodules)
* [Contributing](https://github.com/Swan/Quaver#contributing)
* [Documentation](https://github.com/Swan/Quaver/wiki/Documentation)
* [License](https://github.com/Swan/Quaver#license)

# Status
Quaver is still under development and should not be used for gameplay, and is only intended for developers only. While we do provide builds for users, expect there to be bugs and things not working. Any feature requests should be opened in the [issues](https://github.com/Swan/Quaver/issues).

# Requirements
* Visual Studio 2017 or an equivalent that can compile C# 
* Monogame 3.6
* .NET 4.5
* Up to date submodules

# Submodules
The entire Quaver client is built from a series of class libaries. As such, we have included them as submodules. Please note that some submodules may not be open-source. Aside from that, the following submodules are as such:

* [Quaver-API](https://github.com/Swan/Quaver-API) - Library containing code revolving around map/replay parsing and difficulty calculation.

You can install these submodules by simply running `git submodule update --init --recursive`

# Contributing 
We are welcoming all contributions to our game. The best place to start contributing is by joining our [Discord Server](https://discord.gg/nJa8VFr). All of our main developers, contributors, testers, and community are there and is the best place for real-time collaboration.

To start contributing, we have made it very easy to begin developing. Given that we have private submodules, we have added the "Public" solution configuration which should ultimately remove all privatized library code that is mixed in. When using this, you should be able to compile as normal.

Also, please note that our code is not perfect, but we do have certain standards in place for how we organize our code. It is best to follow those same practices unless you find something completely wrong with it - which in that case we are open for suggestions.

# LICENSE
<a rel="license" href="http://creativecommons.org/licenses/by-nc-nd/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-nd/4.0/88x31.png" /></a>

This work is licensed under a **Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License**. Read more about the license [here](https://creativecommons.org/licenses/by-nc-nd/4.0/).
