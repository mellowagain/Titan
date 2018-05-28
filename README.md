<div align="center">
    <img width=15% src="https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Logo.png">
    <h1>Titan</h1>
</div>

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[![Version](https://img.shields.io/github/release/Marc3842h/Titan/all.svg?label=version)](https://github.com/Marc3842h/Titan/releases)
[![Appveyor](https://img.shields.io/appveyor/ci/Marc3842h/titan-kr2ki.svg?label=windows%20build)](https://ci.appveyor.com/project/Marc3842h/titan-kr2ki)
[![Circle CI](https://img.shields.io/circleci/project/github/Marc3842h/Titan.svg?label=linux%20build)](https://circleci.com/gh/Marc3842h/Titan)
[![Dependencies](https://img.shields.io/librariesio/github/Marc3842h/Titan.svg)](https://libraries.io/github/Marc3842h/Titan)
[![Downloads](https://img.shields.io/github/downloads/Marc3842h/Titan/total.svg)](https://github.com/Marc3842h/Titan/releases)
[![Discord](https://img.shields.io/discord/342308069897928706.svg?label=discord)](https://discord.me/titanbot)
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;

Titan `/ˈtaɪtən/` is a modern report & commendation bot for the 
Source engine. It has been built from the ground up with 
performance and easy-of-use in mind using modern technologies 
like SteamKit.  
  
Report & Commend Bots in Counter-Strike: Global Offensive **are
not patched**. Thanks to regulations from the EU (namely GDPR), Valve
has to show all data, including players that you successfully reported.
Botted players show up on this list so the CS:GO Game Coordinator accepted
these reports which were botted. Valve has been known for trying to patch
report bots in the past but failed as Titan still is working correctly.

## Features

* Support for a graphical user interface (GUI) and command line.
* High performance due to multi-threaded reporting and commending.
* Ban checking for both target and bot accounts. (Requires a generated [Steam Web API](https://steamcommunity.com/dev/apikey) key)
* Integrated Sharecode parser that automatically parses the Match ID from a CS:GO Demo Share URL.
* Support for all Steam ID's known to man (SteamID, SteamID3, SteamID64) as well as Steam profile urls.
* Integrated Match ID resolver that automatically resolves the Match ID from the targets current match.
* Automatic index timer which outputs when an account has finished its 12 hours cooldown.
* Cross-platform compatibility, supports both Windows and Linux. Darwin support is coming soon.
* No installation necessary, every release is provided as binary archive.
* Tracking of botted victims with automatic notification when a ban occurs.
* Automatic Steam Guard code generation from Shared Secrets.
* Steam Guard support.

## Installation

Every version of Titan is provided as binary archive so installation is not required.

#### Dependencies

**Windows**: [.NET Framework ≥4.6.1](https://www.microsoft.com/en-us/download/details.aspx?id=53344) (and for building [Git](https://git-scm.com/), [Visual Studio 2017](https://www.visualstudio.com/downloads/) with .NET Desktop Development tools and [Visual Studio 2017 Build Tools](https://www.visualstudio.com/downloads/#build-tools-for-visual-studio-2017)).  
**Linux**: [Mono ≥5.4](http://www.mono-project.com), [Gtk 3](https://www.gtk.org/),
[libNotify](https://launchpad.net/ubuntu/+source/libnotify) and [libAppindicator 3](https://packages.ubuntu.com/trusty/libappindicator3-dev) (and for building [Git](https://git-scm.com/) and [MsBuild ≥15.0](https://github.com/Microsoft/msbuild)).

#### Option 1: Binary

Download the latest binary from the [releases](https://github.com/Marc3842h/Titan/releases) tab.
Download the package for the appropriate operating system, unpack it and run the `Titan.exe` file.

#### Option 2: From Source

```bash
$ git clone https://github.com/Marc3842h/Titan.git
$ cd Titan

# Run this in a PowerShell terminal on Windows
PS> Set-ExecutionPolicy Unrestricted
PS> .\build.ps1

# Run this in a terminal on Linux
$ chmod +x build.sh
$ ./build.sh
```

#### Option 3: Distro-provided packages

Arch Linux: [`titan-bot-git`](https://aur.archlinux.org/packages/titan-bot-git/)

## Usage

#### Start

Run Titan on Windows by simple double clicking the `Titan.exe` executable.

On Linux, run Titan from the command line using the following syntax:

```bash
$ mono Titan.exe [Verb] [Arguments ...]
```

A list of command line arguments can be found [here](https://github.com/Marc3842h/Titan/blob/master/Titan/Bootstrap/Options.cs).  
If the required arguments have not been supplied, Titan will open the GUI:

![GUI](https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Form.png)

If a recently botted players has been banned, a notification will appear:

![Notification](https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Notification.png)

**NOTE**

By default, Titan will run in the background as a service. To close Titan
completely, click `File > Exit` in the menu bar.

![About](https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/About.png)

<sup>All screenshots have been taken on <b>Arch Linux</b> using <b>Gnome</b> with the <b>Ark-Dark</b> theme.</sup>

#### Converting existing accounts file

Titan brings a Python3 script that can convert an existing `accounts.txt` file (in the format of `username:password`,
from for example [Askwrite's report bot](https://github.com/Askwrite/node-csgo-reportbot)) to a Titan-compatible `accounts.json`
file that can be used with the Titan report & commend bot.

Use it like this:

```python
python convert.py <original accounts file>
```

After it runs successfully a `accounts.json` file can be found in the current directory.

#### i3wm

If i3 window manager is being used, it is recommended to enable floating for Titan in the `.config/i3/config`:

```i3config
for_window [class="Titan"] floating enable
```

#### Accounts file

Here is the syntax of the accounts.json. More information about the syntax can be found on the [wiki](https://github.com/Marc3842h/Titan/wiki/Creating-a-accounts.json).

```js
{
    // A maximum of 11 accounts are allowed per index. Begin a new index when a new account is required.
    "indexes": [
        {
            "accounts": [
                {
                    "username": "username1",
                    "password": "password1",
                    "enabled": true, // May be omitted in order to set it to default value (true)
                    "sentry": false, // May be omitted in order to set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if usage of the shared secret generator is not needed
                },
                {
                    "username": "username11",
                    "password": "password11",
                    "enabled": false, // May be omitted in order to set it to default value (true)
                    "sentry": false, // May be omitted in order to set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if usage of the shared secret generator is not needed
                }
            ]
        },
        {
            "accounts": [
                {
                    "username": "username12",
                    "password": "password12",
                    "enabled": true, // May be omitted in order to set it to default value (true)
                    "sentry": false, // May be omitted in order to set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if usage of the shared secret generator is not needed
                },
                {
                    "username": "username22",
                    "password": "password22",
                    "enabled": true, // May be omitted in order to set it to default value (true)
                    "sentry": false, // May be omitted in order to set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if usage of the shared secret generator is not needed
                }
            ]
        }
    ]
}
```

## Contributing

All contributions are welcomed and appreciated.

#### Bug Reports

Please use the [issue tracker](https://github.com/Marc3842h/Titan/issues) to report any bugs or file feature requests.

#### Developing

Pull Requests are welcome. Restore the NuGet packages (`nuget restore`)
before loading the `.sln` project into an IDE.

#### Donations

Donations are appreciated. 

* Feel free to donate once in a lifetime to my PayPal `accounts \< at \> marcsteiner.me`.
* Feel free to become a monthly pledger on my [Patreon](https://www.patreon.com/marc3842h).  

## License

Titan is licensed under the [MIT License](https://github.com/Marc3842h/Titan/blob/master/LICENSE.txt).
Please visit the `LICENSE.txt` file in the root directory tree for more informations.
All external resources that do not fall unter the MIT license (Images etc.) have been credited
in the `CREDIT.txt` under the Resources directory.
