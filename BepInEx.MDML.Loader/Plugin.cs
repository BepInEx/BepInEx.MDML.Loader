namespace BepInEx.MDML.Loader
{
	[BepInPlugin("io.bepis.mdmlloader", "MuseDashModLoader Loader", "1.0")]
	public class MdmlLoaderPlugin : BaseUnityPlugin
	{
		private void Awake()
		{
			ModLoader.ModLoader.Execute();
		}
	}
}