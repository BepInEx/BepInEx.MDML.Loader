using ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;

namespace ModLoader
{
	public class ModLoader
	{
		private static Dictionary<string, Assembly> dependencies = new Dictionary<string, Assembly>();

		internal static void Execute()
		{
			LoadDependency(typeof(ModLoader).Assembly);

			ModLogger.BepInExLog.LogMessage("Mod loading started");

			try
			{
				string modsDirectory = Path.Combine(Paths.GameRootPath, "MDMLMods");

				if (!Directory.Exists(modsDirectory))
					Directory.CreateDirectory(modsDirectory);

				var mods = LoadMods(modsDirectory);

				foreach (IMod mod in mods)
				{
					ModLogger.BepInExLog.LogMessage($"");
					mod.DoPatching();
				}

				ModLogger.BepInExLog.LogMessage("Finished loading mods");
			}
			catch (Exception ex)
			{
				ModLogger.BepInExLog.LogError($"Failed to load mods: {ex}");
			}
		}

		/// <summary>
		/// Load Mods Dll
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static List<IMod> LoadMods(string path)
		{
			if (!Directory.Exists(path))
				return null;

			List<IMod> mods = new List<IMod>();

			foreach (var dllFile in Directory.GetFiles(path, "*.dll"))
			{
				try
				{
					Assembly assembly = Assembly.LoadFrom(dllFile);

					foreach (Type type in assembly.GetTypes())
					{
						if (typeof(IMod).IsAssignableFrom(type))
						{
							var mod = (IMod)Activator.CreateInstance(type);

							mods.Add(mod);

							ModLogger.BepInExLog.LogDebug($"Discovered mod [{mod.Name}]");
						}
					}

					LoadDependency(assembly);
				}
				catch (Exception ex)
				{
					ModLogger.BepInExLog.LogError($"Failed to load mods from file [{dllFile}]: {ex.Message}");
					ModLogger.BepInExLog.LogDebug(ex);
				}
			}

			return mods;
		}

		private static void LoadDependency(Assembly assembly)
		{
			foreach (string dependStr in assembly.GetManifestResourceNames())
			{
				string filter = $"{assembly.GetName().Name}.Depends.";
				if (dependStr.StartsWith(filter) && dependStr.EndsWith(".dll"))
				{
					string dependName = dependStr.Remove(dependStr.LastIndexOf(".dll")).Remove(0, filter.Length);
					if (dependencies.ContainsKey(dependName))
					{
						ModLogger.BepInExLog.LogError(
							$"Dependency conflict: {dependName} First at: {dependencies[dependName].GetName().Name}");
						continue;
					}

					Assembly dependAssembly;
					using (var stream = assembly.GetManifestResourceStream(dependStr))
					{
						byte[] buffer = new byte[stream.Length];
						stream.Read(buffer, 0, buffer.Length);
						dependAssembly = Assembly.Load(buffer);
					}

					ModLogger.BepInExLog.LogDebug($"Dependency added: {dependName}");

					dependencies.Add(dependName, dependAssembly);
				}
			}
		}
	}
}