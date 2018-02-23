# NetHook 2

A tool for reverse-engineering of the Steam client.  
It's capable of hooking and recording network traffic sent/received by the client.  
  
This tool only works on Windows as its injecting itself into the Windows Steam Client.
  
### Instructions

1. Launch Steam Client (and/or CS:GO)
2. Launch a terminal as admin and type: `rundll32 NetHook2.dll,Inject`.
If you want to inject into CS:GO, type: `rundll32 NetHook2.dll,Inject csgo.exe`
3. Reproduce the feature that you want to reverse-engineer
4. Launch a terminal as admin and type: `rundll32 NetHook2.dll,Eject`
5. Analyze the recorded log (which can be found in your Steam directory) using 
`NetHookAnalyzer2.exe`  
  
### Caution

This tool is only for debugging purposes. It is also not suggested to use it
on your main account as it may risk a `VAC-Ban` (because its injecting itself 
into Steam). Do everything at your own risk.

### Source

* [Original source of NetHook2](https://github.com/SteamRE/SteamKit/tree/master/Resources/NetHook2)
* [Original source of NetHookAnalyzer2](https://github.com/SteamRE/SteamKit/tree/master/Resources/NetHookAnalyzer2)
