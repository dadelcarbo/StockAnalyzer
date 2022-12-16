using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saxo.OpenAPI.AuthenticationServices
{
    public class RandomStringBuilder
    {
        private static readonly List<int> RandomSet = new List<int>();
        static RandomStringBuilder()
        {
            RandomSet.AddRange(Enumerable.Range('A', 26));
            RandomSet.AddRange(Enumerable.Range('a', 26));
            RandomSet.AddRange(Enumerable.Range('0', 10));
            RandomSet.Add('-');
            RandomSet.Add('.');
            RandomSet.Add('_');
            RandomSet.Add('~');
        }

        /// <summary>
        /// Return a random string having characters [A-Z] / [a-z] / [0-9] / "-" / "." / "_" / "~" with the given length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetRandomString(int length)
        {
            if (length <= 0)
                return string.Empty;

            var randomString = new StringBuilder(length);
            var rnd = new Random();

            var buf = new byte[length];
            rnd.NextBytes(buf);

            var enumerator = buf.GetEnumerator();
            while (randomString.Length < length)
            {
                enumerator.MoveNext();
                var index = Convert.ToInt32(enumerator.Current) % RandomSet.Count();
                randomString.Append((char)RandomSet[index]);
            }

            return randomString.ToString();
        }
    }
}
