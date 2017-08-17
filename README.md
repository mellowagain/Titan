<p align="center"><img width=15% src="https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Logo.png"></p>
<p align="center"><img width=40% src="https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Text.png"></p>

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[![Version](https://img.shields.io/github/release/Marc3842h/Titan/all.svg?label=version)](https://github.com/Marc3842h/Titan/releases)
[![Appveyor](https://img.shields.io/appveyor/ci/Marc3842h/titan-kr2ki.svg?label=windows%20build)](https://ci.appveyor.com/project/Marc3842h/titan-kr2ki)
[![Travis CI](https://img.shields.io/travis/Marc3842h/Titan.svg?label=linux%20build)](https://travis-ci.org/Marc3842h/Titan)
[![Dependencies](https://img.shields.io/librariesio/github/Marc3842h/Titan.svg)](https://libraries.io/github/Marc3842h/Titan)
[![Downloads](https://img.shields.io/github/downloads/Marc3842h/Titan/total.svg)](https://github.com/Marc3842h/Titan/releases)
[![Discord](https://img.shields.io/discord/342308069897928706.svg?label=discord)](https://discord.me/titanbot)
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;

Titan `/ˈtaɪtən/` is an advanced Counter-Strike Global Offensive report and commendation bot.
Its goal is to maintain a clean Matchmaking system by sending a target forcefully (by 11 reports) into Overwatch.
It provides a advanced set of features and high effiency when compared against other report and commendation bots.

## Features

* Support for both a graphical user interface (GUI) and command line.
* High performance thanks to multi-threaded reporting and commending (when compared to other report bots).
* Ban checking for both target and bot accounts. (Requires own generated [Steam Web API](https://steamcommunity.com/dev/apikey) key)
* Integrated Sharecode parser that automaticly pauses the Match ID from a CS:GO Demo Share URL
* Automatic index timer which outputs when an account has finished its 6 hours cooldown.
* Cross-platform compatibility, supports both Windows and Linux. Darwin support is coming soon.
* No installation necessary, every release is provided as binary archives.
* Tracking of botted victims with automatic notification when a ban occurs.
* Automatic Steam Guard code generation from Shared Secrets
* Steam Guard support

## Installation

Every version of Titan is provided as binary archives. An installation process is not required.

#### Option 1: Binary

Download the latest binary from the [releases](https://github.com/Marc3842h/Titan/releases) tab.
Download the package for your operating system, unpack it and run the `Titan.exe` file.

#### Option 2: From Source

```
$ git clone https://github.com/Marc3842h/Titan.git
$ cd Titan

# Run this in a PowerShell terminal on Windows
PS> .\build.ps1

# Run this in a bash terminal on Linux
$ chmod +x build.sh && ./build.sh
```


## Usage

#### Accounts file

Create a ```accounts.json``` in main directory with data:
```json
{
    // Per index are maximum 11 accounts allowed. Begin a new index when a new account is required.
    "indexes": [
        {
            "accounts": [
                {
                    "username": "username1",
                    "password": "password1",
                    "enabled": true, // May be omitted if you want set it to default value (true)
                    "sentry": false, // May be omitted if you want set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if you don't want to use the shared secret generator
                },
                {
                    "username": "username11",
                    "password": "password11",
                    "enabled": false, // May be omitted if you want set it to default value (true)
                    "sentry": false, // May be omitted if you want set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if you don't want to use the shared secret generator
                }
            ]
        },
        {
            "accounts": [
                {
                    "username": "username12",
                    "password": "password12",
                    "enabled": true, // May be omitted if you want set it to default value (true)
                    "sentry": false, // May be omitted if you want set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if you don't want to use the shared secret generator
                },
                {
                    "username": "username22",
                    "password": "password22",
                    "enabled": true, // May be omitted if you want set it to default value (true)
                    "sentry": false, // May be omitted if you want set it to default value (false)
                    "secret": "Shared Secret for SteamGuard" // May be omitted if you don't want to use the shared secret generator
                }
            ]
        }
    ]
}
```
#### Start
Run the program from command line with the following syntax:

```bash
$ mono Titan.exe <Arguments>
```

On Windows, run Titan without the `mono` part at the beginning.

You can find a list of command line arguments [here](https://github.com/Marc3842h/Titan/blob/master/Titan/Bootstrap/Options.cs).  
If no (or not enough) arguments have been supplied, Titan will open the GUI:

![GUI](https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Form.png)

If one of your recently botted players got banned, you'll also receive a notification:

![Notification](https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Notification.png)

<sup>All screenshots have been taken on <b>Arch Linux</b> using <b>Gnome</b> with the <b>Ark-Dark</b> theme.</sup>

## Benchmarks

Titan is a multi-threaded C# report bot for Counter-Strike Global Offensive.
Because Titan is multi-threaded, it obviously has an advantage when comparing it to other report bots,
which are mostly one-threaded and written in JavaScript using Node.js.

Benchmarks will follow soon.

## Contributing

All contributions are welcomed and appreciated.

#### Bug Reports

Please use the [issue tracker](https://github.com/Marc3842h/Titan/issues) to report any bugs or file feature requests.

#### Developing

Pull Requests are welcome. I suggest using Rider as IDE for this project, but you're free to choose whatever
you want.

## License

Titan is licensed under the [MIT License](https://github.com/Marc3842h/Titan/blob/master/LICENSE.txt).
Please visit the `LICENSE.txt` file in the root directory tree for more informations.
All external resources that do not fall unter the MIT license (Images etc.) have been credited
in the `CREDIT.txt` under the Resources directory.
