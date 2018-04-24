using System.Collections.Generic;

namespace Titan.Util
{
    public class Cooldown
    {
        
        public static Dictionary<uint, Cooldown> Cooldowns = new Dictionary<uint, Cooldown>();

        public string Reason;
        public bool Permanent;

        public Cooldown(uint penalty, string reason, bool permanent)
        {
            Reason = reason;
            Permanent = permanent;
            Cooldowns.Add(penalty, this);
        }

        public static implicit operator string(Cooldown cooldown)
        {
            return cooldown.Reason;
        }

        public static implicit operator uint(Cooldown cooldown)
        {
            foreach (var keyValue in Cooldowns)
            {
                if (keyValue.Value == cooldown)
                {
                    return keyValue.Key;
                }
            }

            return 0;
        }

        public static implicit operator bool(Cooldown cooldown)
        {
            return cooldown.Permanent;
        }

        public static implicit operator Cooldown(uint penalty)
        {
            foreach (var keyValue in Cooldowns)
            {
                if (keyValue.Key == penalty)
                {
                    return keyValue.Value;
                }
            }

            return null;
        }
        
    }
}
