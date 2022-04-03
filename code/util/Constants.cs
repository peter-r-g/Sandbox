namespace SandboxGame;

public static class Constants
{
	public const string BackupLanguage = "english";
	public const string ProfileDataFileName = "profile_data.json";

	public static class LibraryGroup
	{
		public const string Construction = "construction";
	}

	public static class Command
	{
		public const string EmptyBot = "bot_empty";
		public const string CarAcceleration = "car_acceleration";
		public const string CarDebug = "car_debug";
		public const string CurrentTool = "tool_current";
		public const string DebugTimerStats = "debug_timerstats";
		public const string DebugSetActiveCl = "debug_setactive_cl";
		public const string DebugSetActiveSv = "debug_setactive_sv";
		public const string DebugSaveProfileCl = "debug_saveprofile_cl";
		public const string DebugSaveProfileSv = "debug_saveprofile_sv";
		public const string DebugDumpProfileCl = "debug_dumpprofile_cl";
		public const string DebugDumpProfileSv = "debug_dumpprofile_sv";
		public const string SpawnerEntity = "spawner_entity";
		public const string SpawnerModel = "spawner_model";
	}

	public static class Entity
	{
		public const string Balloon = "entity_balloon";
		public const string BouncyBall = "entity_bouncyball";
		public const string Car = "entity_car";
		public const string Civilian = "entity_civilian";
		public const string CoffeeMug = "entity_coffeemug";
		public const string DirectionalGravity = "entity_directionalgravity";
		public const string Drone = "entity_drone";
		public const string Lamp = "entity_lamp";
		public const string Light = "entity_light";
		public const string Police = "entity_police";
		public const string Thruster = "entity_thruster";
		public const string Wheel = "entity_wheel";
	}

	public static class Tool
	{
		public const string Balloon = "tool_balloon";
		public const string Camera = "tool_camera";
		public const string Color = "tool_color";
		public const string Dresser = "tool_dresser";
		public const string Lamp = "tool_lamp";
		public const string LeafBlower = "tool_leafblower";
		public const string Light = "tool_light";
		public const string Remover = "tool_remover";
		public const string Resizer = "tool_resizer";
		public const string Rope = "tool_rope";
		public const string Spawner = "tool_spawner";
		public const string Thruster = "tool_thruster";
		public const string Weld = "tool_weld";
		public const string Wheel = "tool_wheel";
	}

	public static class Weapon
	{
		public const string Fists = "weapon_fists";
		public const string Flashlight = "weapon_flashlight";
		public const string GravGun = "weapon_gravgun";
		public const string Pistol = "weapon_pistol";
		public const string PhysGun = "weapon_physgun";
		public const string Shotgun = "weapon_shotgun";
		public const string Smg = "weapon_smg";
		public const string Toolgun = "weapon_toolgun";
	}
}
