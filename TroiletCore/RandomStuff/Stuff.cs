using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroiletCore.RandomStuff
{
    internal static class Stuff
    {
        public static void Shuffle<T>(this T[] arr) // stackoverflow on top btw
        {
            int n = arr.Length;
            while(n > 1)
            {
                int k = Helpers.random.Next(n--);
                T temp = arr[n];
                arr[n] = arr[k];
                arr[k] = temp;
            }
        }
    }
}
