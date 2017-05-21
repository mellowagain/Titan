using System;
using System.Text.RegularExpressions;
using Titan.Util;

namespace Titan.Sharecode
{
    public class ShareCodeDecoder
    {

        private string _shareCode;
        private string _dict = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefhijkmnopqrstuvwxyz23456789";

        public ShareCodeDecoder(string shareCode)
        {
            _shareCode = new Regex("CSGO|-").Replace(shareCode, "").Reverse();
        }

        public ShareCodeInfo Decode()
        {
            var result = new byte[18];

            foreach(var c in _shareCode)
            {
                var tmp = new byte[18];

                var addVal = _dict.IndexOf(c);
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
                        tmp[s] =  Convert.ToByte(v & 0xFF);
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
                    result[t] = Convert.ToByte(v & 0xFF);
                }
            }

            return new ShareCodeInfo
            {
                MatchID = BitConverter.ToUInt64(result, 0)
            };
        }

    }
}