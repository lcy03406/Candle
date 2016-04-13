using System;
using System.Collections.Generic;

namespace CandleLib {
	public class World {
		Dictionary<long, Entity> entities;
		public void AddEntity(long id) {
			if (entities.ContainsKey(id))
				return;
			entities.Add(id, new Entity(id, 100));
		}
		public void DelEntity(long id) {
			entities.Remove(id);
		}
		public Entity GetEntity(long id) {
			Entity ent = null;
			entities.TryGetValue(id, out ent);
			return ent;
		}
	}
}

