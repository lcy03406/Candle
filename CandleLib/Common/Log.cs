using System;
using System.Collections.Generic;

namespace CandleLib.Common {
	public enum LogLevel {
		None,
		Error,
		Warn,
		Info,
		Debug,
		Trace,
	}
	public interface LogWriter {
		void WriteLog(string module, LogLevel level, string message);
	}
	public class LogFilter {
		public LogLevel defaultLevel = LogLevel.Trace;
		private Dictionary<string, LogLevel> moduleLevel = new Dictionary<string, LogLevel>();
		public void SetModuleLevel(string module, LogLevel level) {
			moduleLevel[module] = level;
		}
		public void Config(string config) {
			string[] mods = config.Split(',');
			foreach (string mod in mods) {
				string[] pair = mod.Split('=');
				if (pair.Length > 2) {
					throw new ArgumentException(string.Format("invalid config string {0}", mod));
				} else if (pair.Length == 2) {
					string module = pair[0].Trim();
					LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), pair[1].Trim());
					SetModuleLevel(module, level);
				} else if (pair.Length == 1) {
					LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), pair[0].Trim());
					defaultLevel = level;
				}
			}
		}
		public bool Filter(string module, LogLevel level) {
			LogLevel ml;
			if (moduleLevel.TryGetValue(module, out ml)) {
				return level <= ml;
			} else {
				return level <= defaultLevel;
			}
		}
	}
	public class LogConsole : LogWriter {
		public void WriteLog(string module, LogLevel level, string message) {
			Console.WriteLine("{0}:{1}:{2}",module, level, message);
		}
	}
	public static class Logger {
		static LogFilter filter = new LogFilter();
		static LogWriter writer = new LogConsole();
		static public void Config(string config) {
			filter.Config(config);
		}
		static public void SetDefaultLevel(LogLevel level) {
			filter.defaultLevel = level;
		}
		static public void SetModuleLevel(string module, LogLevel level) {
			filter.SetModuleLevel(module, level);
		}
		static public void Log(string module, LogLevel level, string format, params object[] args) {
			if (filter.Filter(module, level)) {
				string message = string.Format(format, args);
				writer.WriteLog(module, level, message);
			}
		}
		static public void Error(string module, string format, params object[] args) {
			Log(module, LogLevel.Error, format, args);
		}
		static public void Warn(string module, string format, params object[] args) {
			Log(module, LogLevel.Warn, format, args);
		}
		static public void Info(string module, string format, params object[] args) {
			Log(module, LogLevel.Info, format, args);
		}
		static public void Debug(string module, string format, params object[] args) {
			Log(module, LogLevel.Debug, format, args);
		}
		static public void Trace(string module, string format, params object[] args) {
			Log(module, LogLevel.Trace, format, args);
		}
	}
}
