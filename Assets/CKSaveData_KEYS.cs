// ------ GENERATED STATIC KEYS FOR DATABOX OBJECT ------
public static class CKSaveData_KEYS
{
	public static class Settings
	{
		public static string TableName = "Settings"; 

		public static class Screen
		{
			public static string EntryName = "Screen";
			/// <summary>
			/// Type of: IntType
			/// </summary>
			public static string _Resolution = "Resolution";
			/// <summary>
			/// Type of: BoolType
			/// </summary>
			public static string _Fullscreen = "Fullscreen";
		}
		public static class Volume
		{
			public static string EntryName = "Volume";
			/// <summary>
			/// Type of: FloatType
			/// </summary>
			public static string _Master = "Master";
			/// <summary>
			/// Type of: FloatType
			/// </summary>
			public static string _SFX = "SFX";
			/// <summary>
			/// Type of: FloatType
			/// </summary>
			public static string _Music = "Music";
		}
		public static class Accessibility
		{
			public static string EntryName = "Accessibility";
			/// <summary>
			/// Type of: IntType
			/// </summary>
			public static string _Colorblind_Mode = "Colorblind Mode";
			/// <summary>
			/// Type of: BoolType
			/// </summary>
			public static string _Subtitles = "Subtitles";
			/// <summary>
			/// Type of: BoolType
			/// </summary>
			public static string _Tooltips = "Tooltips";
		}
	}
	public static class Progression
	{
		public static string TableName = "Progression"; 

		public static class Last_Level_Completed
		{
			public static string EntryName = "Last Level Completed";
			/// <summary>
			/// Type of: StringType
			/// </summary>
			public static string _Scene_Name = "Scene Name";
		}
		public static class Completed_Levels
		{
			public static string EntryName = "Completed Levels";
			/// <summary>
			/// Type of: BoolType
			/// </summary>
			public static string _Scene_Name = "Scene Name";
		}
		public static class NPC_Dialogue
		{
			public static string EntryName = "NPC Dialogue";
			/// <summary>
			/// Type of: IntType
			/// </summary>
			public static string _Scene_Name_Count = "Scene Name Count";
		}
		public static class Collectables
		{
			public static string EntryName = "Collectables";
			/// <summary>
			/// Type of: BoolType
			/// </summary>
			public static string _Scene_Name_Found = "Scene Name Found";
		}
		public static class Scene_Loaded_From
		{
			public static string EntryName = "Scene Loaded From";
			/// <summary>
			/// Type of: StringType
			/// </summary>
			public static string _Scene_Name = "Scene Name";
		}
		public static class Loaded_From_Pause
		{
			public static string EntryName = "Loaded From Pause";
			/// <summary>
			/// Type of: BoolType
			/// </summary>
			public static string _Loaded_From_Pause = "Loaded From Pause";
		}
	}
}