using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoClient {
	class Program {
		static void Main(string[] args) {
			CandleLib.Echo.Client module = new CandleLib.Echo.Client();
			module.Run();
		}
	}
}
