using System;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Serilog.Core;
using Titan.Logging;
using Titan.Util;

namespace Titan.Sharecode
{
    
    [Credit("akiver/CSGO-Demos-Manager")]
    public class ShareCode
    {

        private static Logger _log = LogCreator.Create();

        private static string _dictionary = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefhijkmnopqrstuvwxyz23456789";
        private static Regex _regex = new Regex("^CSGO(-?[\\w]{5}){5}$");

        public static ShareCodeInfo Decode(string shareCode)
        {
            if(_regex.IsMatch(shareCode))
            {
                var code = shareCode.Remove(0, 4).Replace("-", "");
                
                var big = code.Reverse().Aggregate(BigInteger.Zero, (current, c) => 
                    BigInteger.Multiply(current, _dictionary.Length) + _dictionary.IndexOf(c));

                var matchIdBytes = new byte[sizeof(ulong)];
                var outcomeIdBytes = new byte[sizeof(ulong)];
                var tvPortIdBytes = new byte[sizeof(uint)];

                var all = big.ToByteArray().Reverse().ToArray();
                Array.Copy(all, 0, matchIdBytes, 0, sizeof(ulong));
                Array.Copy(all, sizeof(ulong), outcomeIdBytes, 0, sizeof(ulong));
                Array.Copy(all, 2 * sizeof(ulong), tvPortIdBytes, 0, sizeof(ushort));
                
                return new ShareCodeInfo
                {
                    MatchID = BitConverter.ToUInt64(matchIdBytes, 0),
                    OutcomeID = BitConverter.ToUInt64(outcomeIdBytes, 0),
                    Tokens = BitConverter.ToUInt32(tvPortIdBytes, 0)
                };
            }
            
            _log.Error("Could not decode {ShareCode} to valid ShareCodeInfo.", shareCode);
            return null;
        }

        public static string Encode(ShareCodeInfo info)
        {
            var matchIdBytes = BitConverter.GetBytes(info.MatchID);
            var reservationBytes = BitConverter.GetBytes(info.OutcomeID);
            var tvPort16 = (ushort)(info.Tokens & ((1 << 16) - 1));
            var tvBytes = BitConverter.GetBytes(tvPort16);

            var bytes = new byte[matchIdBytes.Length + reservationBytes.Length + tvBytes.Length + 1];

            Buffer.BlockCopy(new byte[] { 0 }, 0, bytes, 0, 1);
            Buffer.BlockCopy(matchIdBytes, 0, bytes, 1, matchIdBytes.Length);
            Buffer.BlockCopy(reservationBytes, 0, bytes, 1 + matchIdBytes.Length, reservationBytes.Length);
            Buffer.BlockCopy(tvBytes, 0, bytes, 1 + matchIdBytes.Length + reservationBytes.Length, tvBytes.Length);

            var big = new BigInteger(bytes.Reverse().ToArray());

            var charArray = _dictionary.ToCharArray();
            var c = "";

            for(var i = 0; i < 25; i++)
            {
                BigInteger rem;
                BigInteger.DivRem(big, charArray.Length, out rem);
                c += charArray[(int)rem];
                big = BigInteger.Divide(big, charArray.Length);
            }

            return $"CSGO-{c.Substring(0, 5)}-{c.Substring(5, 5)}-{c.Substring(10, 5)}-{c.Substring(15, 5)}-{c.Substring(20, 5)}";
        }
    }

    public class CreditAttribute : Attribute
    {
        public CreditAttribute(string @null) {}
    }
    
}