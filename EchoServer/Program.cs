using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer {
	class Program {
		static void Main(string[] args) {
			CandleLib.Echo.Server module = new CandleLib.Echo.Server();
			module.Run();
		}
	}
}
