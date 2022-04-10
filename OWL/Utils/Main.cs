using Microsoft.Xna.Framework;
using OWL.Graph;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
#if __ANDROID__
using Android.Content;
#elif __IOS__
using UIKit;
#endif

/// <summary>
/// The main Util class.
/// Contains all methods for performing basic utility functions.
/// </summary>
namespace OWL.Util
{
    public static class Utils
    {
#if __ANDROID__ || __IOS__
        public static bool isMobile = true;
#else
        public static bool isMobile = false;
#endif

        /// <summary>
        /// takes a number and returns a padded string
        /// </summary>
        /// <param name="n">N.</param>
        /// <param name="width">Width.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PadNum(int n, int width = 0)
        {
            var num = Math.Abs(n);
            var zeros = Math.Max(0, width - Mathf.FastFloorToInt(num.ToString().Length));
            var zeroStr = Math.Pow(10, zeros).ToString().Substring(1);
            if (num < 0)
            {
                zeroStr = "-" + zeroStr;
            }
            return zeroStr + num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object SelectRandom(List<object> array)
        {
            return array[Mathf.RNG.Next(0, array.Count)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object SelectRandom(object[] array)
        {
            return array[Mathf.RNG.Next(0, array.Length)];
        }

        public static T SelectRandom<T>(List<T> a)
        {
            return (T)a[Mathf.RNG.Next(0, a.Count)];
        }

        public static T SelectRandom<T>(T[] a)
        {
            return (T)a[Mathf.RNG.Next(0, a.Length)];
        }

        /// <summary>
        /// Simple hashing function takes a string and returns and integer
        /// </summary>
        /// <returns>string hashed to integer</returns>
        /// <param name="seed">F.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashedValue(string seed)
        {
            int hash = 0;
            if (seed.Length == 0)
            {
                return hash;
            }

            for (int i = 0; i < seed.Length; i++)
            {
                int code = char.ConvertToUtf32(seed, i);
                hash = ((hash << 5) - hash) + code;
                hash = hash & hash; // convert to 32 bit integer
            }
            return Math.Abs(hash);
        }

        public static void OpenURL(string url)
        {
#if __ANDROID__
            var uri = Android.Net.Uri.Parse(url);
            var intent = new Intent(Intent.ActionView, uri);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
#elif __IOS__
            UIApplication.SharedApplication.OpenUrl(new Foundation.NSUrl(url));
#else
            System.Diagnostics.Process.Start(url);
#endif
        }

        public static bool Collide(Container a, Container b)
        {
            var r1 = a.GetBounds();
            var r2 = b.GetBounds();

            return !(r2.X > (r1.X + r1.Width) || (r2.X + r2.Width) < r1.X || 
                r2.Y > (r1.Y + r1.Height) || (r2.Y + r2.Height) < r1.Y);
        }
    }
}