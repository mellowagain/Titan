using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Eto.Forms;
using Newtonsoft.Json;
using Quartz;
using Serilog.Core;
using SteamKit2;
using Titan.Json;
using Titan.Util;

namespace Titan.Logging
{
    public class VictimTracker : IJob
    {

        // This class tracks victims of reporting sessions and
        // reports as soon as a victims get banned.
        
        private Logger _log = LogCreator.Create();
        
        private List<Victims.Victim> _victims;

        private FileInfo _file = new FileInfo(
            Path.Combine(Environment.CurrentDirectory, "victims.json"));
        
        public IJobDetail Job = JobBuilder.Create<VictimTracker>()
            .WithIdentity("Victim Tracker Job", "Titan")
            .Build();
        
        public ITrigger Trigger = TriggerBuilder.Create()
            .WithIdentity("Victim Tracker Trigger", "Titan")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(15) // CHange to 15 Minutes
                .RepeatForever())
            .Build();

        public VictimTracker()
        {
            _victims = GetVictimsFromFile();
        }
        
        public void AddVictim(SteamID steamID)
        {
            _victims.Add(new Victims.Victim
            {
                SteamID = steamID.ConvertToUInt64(),
                Ticks = DateTime.Now.Ticks
            });
        }

        public List<Victims.Victim> GetVictimsFromFile()
        {
            if(!_file.Exists)
            {
                _file.Create();
                SaveVictimsFile();
            }
            
            using(var reader = File.OpenText(_file.ToString()))
            {
                var json = (Victims) new JsonSerializer().Deserialize(reader, typeof(Victims));

                if(json?.Array != null)
                {
                    var victims = new List<Victims.Victim>(json.Array);

                    return victims;
                }
                return new List<Victims.Victim>();
            }
        }

        public void SaveVictimsFile()
        {
            using(var writer = new StreamWriter(_file.OpenWrite(), Encoding.UTF8))
            {
                var victims = new Victims
                {
                    Array = _victims.ToArray()
                };
                
                writer.Write(JsonConvert.SerializeObject(victims, Formatting.Indented));
            }

            _log.Debug("Successfully wrote Victim file.");
        }

        // Quartz.NET Job Executor
        public void Execute(IJobExecutionContext context)
        {
            _log.Debug("Checking all victims if they have bans on record.");
            
            _log.Information("Victims: {a}", _victims);
            
            foreach(var victim in _victims.ToArray())
            {
                var target = SteamUtil.FromSteamID64(victim.SteamID);
                var bans = Titan.Instance.BanManager.GetBanInfoFor(target);
                var time = DateTime.Now.Subtract(new DateTime(victim.Ticks));
                
                if(bans.GameBanCount > 0 || bans.VacBanned)
                {
                    var count = bans.GameBanCount == 0 ? bans.VacBanCount : bans.GameBanCount;
                    var id64 = target.ConvertToUInt64();
                    
                    _victims.Remove(victim);
                    
                    _log.Information("Your recently botted target {Target} received " +
                                     "{Count} ban(s) after {Delay}. Thank you for using Titan.",
                        id64, count, time.Hours == 0 ? time.Minutes + " minute(s)" : time.Hours + " hour(s)");
                    
                    var notification = new Notification
                    {
                        Title = "Titan - " + id64 + " banned",
                        Message = "Your recently botted target " + id64 + " " +
                                  "has been banned and has now " + count + " Ban(s) on record.",
                        Icon = Titan.Instance.UIManager.SharedResources.TITAN_ICON
                    };
                    
                    notification.Activated += delegate
                    {
                        Process.Start("https://steamcommunity.com/profiles/" + id64 + "/");
                    };

                    Application.Instance.Invoke(() => notification.Show());
                }
            }
        }
        
    }
}