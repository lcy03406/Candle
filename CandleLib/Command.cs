using System;

namespace CandleLib {
	interface Command {
		void Do(World world);
	}

	public class Login : Command {
		public long id;
		public void Do(World world) {
			world.AddEntity(id);
		}
	}

	public class Logout : Command {
		public long id;
		public void Do(World world) {
			world.DelEntity(id);
		}
	}

	public class Bite : Command {
		public long id;
		public long victim;
		public void Do(World world) {
			Entity src = world.GetEntity(id);
			Entity dst = world.GetEntity(victim);
			if (src == null || dst == null)
				return;
			src.Bite(dst);
		}
	}
}

