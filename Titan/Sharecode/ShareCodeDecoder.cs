using System;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Core;
using Titan.Logging;
using Titan.Util;

namespace Titan.Sharecode
{
    public class ShareCodeDecoder
    {

        private Logger _log = LogCreator.Create();

        private string _dic = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefhijkmnopqrstuvwxyz23456789";

        private string _shareCode;
        private string _nonsanitizedCode;

        public ShareCodeDecoder(string shareCode)
        {
            _shareCode = Sanitize(shareCode);
            _nonsanitizedCode = shareCode;
        }

        public ShareCodeInfo Decode()
        {
            var result = new int[18];

            foreach(var c in _shareCode)
            {
                var tmp = new int[18];

                var addVal = _dic.IndexOf(c);
                int carry;
                int v;

                for(var t = 17; t >= 0; t--)
                {
                    carry = 0;

                    for(var s = t; s >= 0; s--)
                    {
                        if(t - s == 0)
                        {
                            v = tmp[s] + result[t] * 57;

                        }
                        else
                        {
                            v = 0;
                        }
                        v = v + carry;
                        carry = v >> 8;
                        tmp[s] = v & 0xFF;
                    }
                }

                result = tmp;
                carry = 0;

                for(var t = 17; t >= 0; t--)
                {
                    if(t == 17)
                    {
                        v = result[t] + addVal;
                    }
                    else
                    {
                        v = result[t];
                    }
                    v = v + carry;
                    carry = v >> 8;
                    result[t] = v & 0xFF;
                }
            }

            return new ShareCodeInfo
            {
                // TODO: FIXME
                MatchId = Convert.ToUInt64(result
                    .Select((t, i) => t * Convert.ToInt32(Math.Pow(10, result.Length - i - 1)))
                    .Sum()),
                OutcomeId = Convert.ToUInt64(result
                    .Select((t, i) => t * Convert.ToInt32(Math.Pow(10, result.Length - i - 1)))
                    .Sum() + 8),
                Token = Convert.ToInt16(result
                    .Select((t, i) => t * Convert.ToInt32(Math.Pow(10, result.Length - i - 1)))
                    .Sum() + 17)
            };
        }

        public string Sanitize(string shareCode)
        {
            var regex = new Regex("CSGO|-");

            return regex.Replace(shareCode, "").Reverse();
        }



    }
}