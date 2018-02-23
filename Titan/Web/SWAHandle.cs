using System;
using System.Collections.Generic;
using System.Net;
using Serilog.Core;
using SteamKit2;
using Titan.Bans;
using Titan.Logging;

namespace Titan.Web
{
    // Steam Web API Handle
    public class SWAHandle
    {

        public Logger Log = LogCreator.Create();

        private KeyManager _keyManager;

        public SWAHandle()
        {
            _keyManager = new KeyManager(this);
        }

        public bool RequestBanInfo(SteamID steamID, out BanInfo banInfo)
        {
            if (steamID != null && !string.IsNullOrEmpty(_keyManager.SWAKey))
            {
                try
                {
                    using (dynamic steamUser = WebAPI.GetInterface("ISteamUser", _keyManager.SWAKey))
                    {
                        KeyValue pair = steamUser.GetPlayerBans(steamids: steamID.ConvertToUInt64());

                        foreach (var get in pair["players"].Children)
                        {
                            if (get["SteamId"].AsUnsignedLong() == steamID.ConvertToUInt64())
                            {
                                banInfo = new BanInfo
                                {
                                    SteamID = get["SteamId"].AsUnsignedLong(),
                                    CommunityBanned = get["CommunityBanned"].AsBoolean(),
                                    VacBanned = get["VACBanned"].AsBoolean(),
                                    VacBanCount = get["NumberOfVACBans"].AsInteger(),
                                    DaysSinceLastBan = get["DaysSinceLastBan"].AsInteger(),
                                    GameBanCount = get["NumberOfGameBans"].AsInteger(),
                                    EconomyBan = get["EconomyBan"].AsString()
                                };
                                return true;
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Log.Error(ex, "A error occured when trying to get the Ban Information for {SteamID}.",
                              steamID.ConvertToUInt64());
                }

                Log.Warning("Did not receive ban informations for {SteamID}. Skipping...", steamID.ConvertToUInt64());
                
                banInfo = null;
                return false;
            }

            Log.Warning("No valid Web API key has been found. Skipping ban checking...");
            
            banInfo = null;
            return false;
        }

        public bool RequestSteamUserInfo(string vanityURL, out ulong steamID64)
        {
            try
            {
                using (dynamic steamUser = WebAPI.GetInterface("ISteamUser", _keyManager.SWAKey))
                {    
                    KeyValue pair = steamUser.ResolveVanityURL(vanityurl: vanityURL);

                    if (pair["success"].AsInteger() == 1)
                    {
                        steamID64 = pair["steamid"].AsUnsignedLong();
                        
                        return true;
                    }
                    
                    Log.Error("Could not resolve custom URL {URL} to SteamID64: {Error}",
                              vanityURL, pair["message"].AsString());
                }
            }
            catch (WebException ex)
            {
                Log.Error("Could not resolve custom URL {URL} to SteamID64: {Error}",
                          vanityURL, ex.Message);
            }

            steamID64 = 0;
            return false;
        }

        // Used for checking if Steam Web API key is valid
        public bool TestAPIKey()
        {
            try
            {
                using (dynamic steamUser = WebAPI.GetInterface("ISteamUser", _keyManager.SWAKey))
                {
                    KeyValue pair = steamUser.GetPlayerSummaries(steamids: "76561198224231904");
                    
                    return pair != null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("403") && ex.Message.Contains("Forbidden"))
                {
                    Log.Error("Steam returned {error}. This Steam Web API key is invalid!", ex.Message);
                }
            }

            return false;
        }

        public void Load()
        {
            _keyManager.Load();
        }

        public void Save()
        {
            _keyManager.Save();
        }

        public void SetKey(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                _keyManager.SWAKey = key;

                if (!TestAPIKey())
                {
                    _keyManager.SWAKey = null;
                    Log.Warning("Received invalid Steam Web API key. Ignoring...");
                }
                else
                {
                    Log.Information("Steam Web API key was valid. Enjoy using Titan!");
                    Save();
                }
            }
        }

        public string GetKey()
        {
            return _keyManager.SWAKey;
        }

    }
}