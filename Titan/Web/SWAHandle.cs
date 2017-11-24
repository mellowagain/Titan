using System;
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

        public BanInfo RequestBanInfo(SteamID steamID)
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
                                return new BanInfo
                                {
                                    SteamID = get["SteamId"].AsUnsignedLong(),
                                    CommunityBanned = get["CommunityBanned"].AsBoolean(),
                                    VacBanned = get["VACBanned"].AsBoolean(),
                                    VacBanCount = get["NumberOfVACBans"].AsInteger(),
                                    DaysSinceLastBan = get["DaysSinceLastBan"].AsInteger(),
                                    GameBanCount = get["NumberOfGameBans"].AsInteger(),
                                    EconomyBan = get["EconomyBan"].AsString()
                                };
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
                return null;
            }

            Log.Warning("No valid Web API key has been found. Skipping ban checking...");
            return null;
        }

        public bool RequestSteamUserInfo(string vanityURL, out ulong steamID64)
        {
            try
            {
                using(dynamic steamUser = WebAPI.GetInterface("ISteamUser", _keyManager.SWAKey))
                {    
                    KeyValue pair = steamUser.ResolveVanityURL(vanityurl: vanityURL);

                    if(pair["success"].AsInteger() == 1)
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
        public bool RequestAPIMethods()
        {
            try
            {
                using (dynamic steamWebAPIUtil = WebAPI.GetInterface("ISteamWebAPIUtil", _keyManager.SWAKey))
                {
                    KeyValue pair = steamWebAPIUtil.GetSupportedAPIList0001();

                    if (pair != null)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            Log.Error("Invalid response received.");
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

                if (!RequestAPIMethods())
                {
                    _keyManager.SWAKey = null;
                    Log.Warning("Received invalid Steam Web API key. Ignoring...");
                }
            }
        }

        public string GetKey()
        {
            return _keyManager.SWAKey;
        }

    }
}