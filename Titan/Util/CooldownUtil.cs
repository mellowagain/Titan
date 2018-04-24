namespace Titan.Util
{
    public class CooldownUtil
    {
        
        public Cooldown CooldownNone = new Cooldown(0, "Temporary Matchmaking Cooldown (No reason)", false);
        public Cooldown Kicked = new Cooldown(1, "You have been kicked from your last matchmaking game.", false);
        public Cooldown KilledMate = new Cooldown(2, "You killed too many teammates.", false);
        public Cooldown RoundStartKill = new Cooldown(3, "You killed a teammate at round start.", false);
        public Cooldown FailReconnect = new Cooldown(4, "You failed to reconnect to your last match.", false);
        public Cooldown Abandon = new Cooldown(5, "You abandoned your last match.", false);
        public Cooldown DamagedMate = new Cooldown(6, "You dealt too much damage to your teammates.", false);
        public Cooldown DamagedMateStart = new Cooldown(7, "You dealt too much damage to your teammates at round start.", false);
        public Cooldown UntrustedAngles = new Cooldown(8, "Your account is permanently untrusted. (Illegal Angles)", true);
        public Cooldown KickedTooMuch = new Cooldown(9, "You were kicked from too many recent matches.", false);
        public Cooldown MajorlyDisruptive = new Cooldown(10, "Convicted by Overwatch: Majorly Disruptive", true);
        public Cooldown MinorlyDisruptive = new Cooldown(11, "Convicted by Overwatch: Minorly Disruptive", false);
        public Cooldown ResolveState = new Cooldown(12, "Resolving Matchmaking state for your account.", false);
        public Cooldown ResolveStateLastMatch = new Cooldown(13, "Resolving Matchmaking state for your last match.", false);
        public Cooldown UntrustedVac = new Cooldown(14, "Your account is permanently untrusted. (VAC)", true);
        public Cooldown PermanentCooldownNone = new Cooldown(15, "Permanent Matchmaking Cooldown (No reason)", true);
        public Cooldown FailConnect = new Cooldown(16, "You failed to connect by match start.", false);
        public Cooldown KickedMates = new Cooldown(17, "You kicked too many teammates in recent matches.", false);
        public Cooldown NewbieCooldown = new Cooldown(18, "Your account is under skill placement calibration.", false);
        public Cooldown GameServerBanned = new Cooldown(19, "A server using your game server token has been banned.", true);
        
    }
}
