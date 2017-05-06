using System.Linq;

namespace Titan.Util
{
    public static class StringUtil
    {

        public static string Reverse(this string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }

    }
}