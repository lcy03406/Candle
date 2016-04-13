using System;

namespace CandleServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			CandleLib.Hello hello = new CandleLib.Hello("world");
			Console.WriteLine (hello.Say());
		}
	}
}
