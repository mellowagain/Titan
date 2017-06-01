using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eto.Forms;
using Newtonsoft.Json;
using Quartz;
using Serilog.Core;
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
            Path.Combine(Environment.CurrentDirectory, "victim-tracking.json"));
        
        public IJobDetail Job = JobBuilder.Create<VictimTracker>()
            .WithIdentity("Victim Tracker Job", "Titan")
            .Build();
        
        public ITrigger Trigger = TriggerBuilder.Create()
            .WithIdentity("Victim Tracker Trigger", "Titan")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(15)
                .RepeatForever())
            .Build();

        public void ParseVictimsFile()
        {
            if(!_file.Exists)
            {
                _file.Create();
            }

            using(var reader = File.OpenText(_file.ToString()))
            {
                var json = (Victims) new JsonSerializer().Deserialize(reader, typeof(Victims));

                _victims = json.Array.ToList();
            }
        }

        public void SaveVictimsFile()
        {
            
        }

        // Quartz.NET Job Executor
        public void Execute(IJobExecutionContext context)
        {
            foreach(var victim in _victims)
            {
                var target = SteamUtil.FromSteamID64(victim.SteamID);
                var bans = Titan.Instance.BanManager.GetBanInfoFor(target);

                if(bans.GameBanCount > 0)
                {
                    _log.Information("Your recently botted target {Target} has now " +
                                     "{Count} game ban(s) on record. Thank you for using Titan.",
                        target.ConvertToUInt64(), bans.GameBanCount);

                    MessageBox.Show("Congratulations! Your recently botted target " +
                                    target.ConvertToUInt64() + " has now " + bans.GameBanCount + " " +
                                    "Game Ban(s) on record.", "Titan - Confirmed Ban");
                }
            }
        }
        
    }
}