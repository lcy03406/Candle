using System;

namespace CandleLib {
	public class Entity {
		long id;
		int hp;
		public Entity (long id, int hp) {
			this.id = id;
			this.hp = hp;
		}

		public void Bite(Entity other) {
			if (other.hp > 0) {
				other.hp--;
			}
		}
	}
}

