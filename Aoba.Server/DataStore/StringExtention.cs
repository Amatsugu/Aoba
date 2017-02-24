using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.DataStore
{
	public static class StringExtention
	{
		public static readonly char[] BASE60_CHARS = new char[]
		{
			'0','1','2','3','4','5','6','7','8','9',
			'A','B','C','D','E','F','G','H','I','J',
			'K','L','M','N','O','P','Q','R','S','T',
			'U','V','W','X','Y','Z','a','b','c','d',
			'e','f','g','h','i','j','k','l','m','n',
			'o','p','q','r','s','t','u','v','w','x'
		};

		//String to base 60
		public static string ToBase60(this string value) => value.ToLower().GetHashCode().ToBase60();

		//Int to base 60
		public static string ToBase60(this int value) => ((long)value).ToBase60();

		//Convert to Base60
		public static string ToBase60(this long value)
		{
			bool neg = false;
			if (value < 0)
			{
				value = -value;
				neg = true;
			}
			int i = 64;


			char[] buffer = new char[i];
			int targetBase = BASE60_CHARS.Length;

			do
			{
				buffer[--i] = BASE60_CHARS[value % targetBase];
				value = value / targetBase;
			}
			while (value > 0);

			char[] result = new char[64 - i];
			Array.Copy(buffer, i, result, 0, 64 - i);

			string output = new string(result);
			return (neg) ? $"~{output}" : output;
		}
	}
}
