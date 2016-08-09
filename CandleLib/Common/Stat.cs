using System;
using System.Collections.Generic;

namespace CandleLib.Common {
	public class Statistic<T> where T : IComparable {
		class Stat {
			public int first;
			public int last;
			public int min;
			public int max;
			public int sum;
			public int count;
		}
		SortedList<T, Stat> stats = new SortedList<T, Stat>();

		public void Clear() {
			stats.Clear();
		}

		public void Add(T tag, int value) {
			Stat st;
			if (!stats.TryGetValue(tag, out st)) {
				st = new Stat();
				st.first = value;
				stats.Add(tag, st);
			}
			st.count += 1;
			st.sum += value;
			st.last = value;
			if (value < st.min)
				st.min = value;
			if (value > st.max)
				st.max = value;
		}

		public void Print() {
			foreach (KeyValuePair<T, Stat> pair in stats) {
				T tag = pair.Key;
				Stat st = pair.Value;
				int avg = 0;
				if (st.count > 0)
					avg = st.sum / st.count;
				Logger.Info("stat", "Tag={0}, first={1}, last={2}, min={3}, max={4}, sum={5}, count={6}, avg={7}",
					tag, st.first, st.last, st.min, st.max, st.sum, st.count, avg);
			}
		}
	}
}
