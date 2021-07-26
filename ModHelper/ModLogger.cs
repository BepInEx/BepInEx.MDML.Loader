using System.Diagnostics;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

[assembly: InternalsVisibleTo("BepInEx.MDML.Loader")]

namespace ModHelper
{
	public static class ModLogger
	{
		internal static ManualLogSource BepInExLog { get; }

		static ModLogger()
		{
			BepInExLog = Logger.CreateLogSource("MDML");
		}

		public static void Debug(object obj)
		{
			var frame = new StackTrace().GetFrame(1);
			var className = frame.GetMethod()?.ReflectedType?.Name ?? "<null>";
			var methodName = frame.GetMethod().Name;
			AddLog(className, methodName, obj);
		}

		public static void AddLog(string className, string methodName, object obj)
		{
			var text = $"[{className}:{methodName}]: {obj}";

			BepInExLog.LogMessage(text);
		}
	}
}