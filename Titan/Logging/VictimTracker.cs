using System;
using System.Collections.Generic;
using System.IO;
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
            if(_file.Exists)
            {
                _file.Delete();
            }

            var victims = new Victims
            {
                Array = _victims.ToArray()
            };

            using(var writer = File.CreateText(_file.ToString()))
            {
                var str = JsonConvert.SerializeObject(victims, Formatting.Indented);
                writer.Write(str);
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
                
                _log.Information("Ban Info for {Victim}: {BanInfo}",
                    target.ConvertToUInt64(), bans);
                
                if(bans.GameBanCount > 0 || bans.VacBanned)
                {
                    _log.Information("Your recently botted target {Target} received " +
                                     "{Count} ban(s) after {Delay}. Thank you for using Titan.",
                        target.ConvertToUInt64(), bans.GameBanCount == 0 ? bans.VacBanCount : bans.GameBanCount, 
                        time.Hours == 0 ? time.Minutes + " minute(s)" : time.Hours + " hour(s)");

                    // TODO: Replace this with a System Notification
                    // Windows & some Linux DE (libnotify) has support for notifications
                    Application.Instance.Invoke(() => MessageBox.Show("Congratulations! Your recently botted target " +
                                                                      target.ConvertToUInt64() + " has now " +
                                                                      bans.GameBanCount + " " +
                                                                      "Ban(s) on record.", "Titan - Confirmed Ban"));
                    
                    _victims.Remove(victim);
                }
            }
        }
        
    }
}