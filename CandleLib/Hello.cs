using System;

namespace CandleLib {
	public class Hello {
		string who;
		public Hello(string who) {
			this.who = who;
		}
		public string Say() {
			return string.Format("hello, {0}", who);
		}
	}
}

