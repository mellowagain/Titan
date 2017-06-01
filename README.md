<p align="center"><img width=15% src="https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Logo.png"></p>
<p align="center"><img width=40% src="https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/Text.png"></p>

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[![Version](https://img.shields.io/github/release/Marc3842h/Titan/all.svg?label=version)](https://github.com/Marc3842h/Titan/releases)
[![Appveyor](https://img.shields.io/appveyor/ci/Marc3842h/titan-kr2ki.svg?label=windows%20build)](https://ci.appveyor.com/project/Marc3842h/titan-kr2ki)
[![Travis CI](https://img.shields.io/travis/Marc3842h/Titan.svg?label=linux%20build)](https://travis-ci.org/Marc3842h/Titan)
[![Dependencies](https://img.shields.io/librariesio/github/Marc3842h/Titan.svg)](https://libraries.io/github/Marc3842h/Titan)
[![Downloads](https://img.shields.io/github/downloads/Marc3842h/Titan/total.svg)](https://github.com/Marc3842h/Titan/releases)
[![License](https://img.shields.io/github/license/Marc3842h/Titan.svg)](https://github.com/Marc3842h/Titan/blob/master/LICENSE.txt)
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;

Titan `/ˈtaɪtən/` is an advanced Counter-Strike Global Offensive report and commendation bot.
Its goal is to maintain a clean Matchmaking system by sending a target forcefully (by 11 reports) into Overwatch.
It provides a advanced set of features and high effiency when compared against other report and commendation bots.

**Development will be hold until the 15th of June 2017 as [Glacier](github.com/Marc3842h/Glacier) takes priority.
Support will still be available on the issues tracker and Pull Requests will be merged**.

## Features

* Support for both a graphical user interface (GUI) and command line.
* High performance thanks to multi-threaded reporting and commending (when compared to other report bots).
* Ban checking for both target and bot accounts. (Requires own generated [Steam Web API](https://steamcommunity.com/dev/apikey) key)
* Integrated Sharecode parser that automaticly pauses the Match ID from a CS:GO Demo Share URL
* Automatic index timer which outputs when an account has finished its 6 hours cooldown.
* Cross-platform compatibility, supports both Windows and Linux. Darwin support is coming soon.
* No installation necessary, every release is provided as binary archives.
* Steam Guard support

## Installation

Every version of Titan is provided as binary archives. An installation process is not required.

#### Option 1: Binary

Download the latest binary from the [releases](https://github.com/Marc3842h/Titan/releases) tab.
Download the package for your operating system, unpack it and run the `Titan.exe` file.

#### Option 2: From Source

```
$ git clone git@github.com:Marc3842h/Titan.git
$ cd Titan

# Run this in a PowerShell terminal on Windows
PS> .\build.ps1

# Run this in a bash terminal on Linux
$ chmod +x build.sh && ./build.sh
```

## Usage

Start the program from command line with the following syntax:

```bash
$ mono Titan.exe (--target) (--mode) [--id] [--file]
```

On Windows, run Titan without the `mono` part at the beginning.

If no arguments have been passed with Titan, the GUI will open:

![GUI](https://github.com/Marc3842h/Titan/blob/master/Titan/Resources/MainForm.png)

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
