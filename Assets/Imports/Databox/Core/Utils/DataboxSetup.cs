#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager;


public class DataboxSetup
{	

	public static readonly string [] defineSymbolsAddressables = new string[]
	{
		"DATABOX_ADDRESSABLES",
	};
	
	static string assemblyDefinition =
		"{ " + "\n" +
		"\"name\": \"Databox\"," + "\n" +
		"\"references\": [" + "\n" +
		"\"Databox.FullSerializer\"," + "\n" +
		"\"Databox.OdinSerializer\"," + "\n" +
		"\"Unity.Addressables.Editor\"," + "\n" +
		"\"Unity.Addressables\"," + "\n" +
		"\"Unity.ResourceManager\"" + "\n" +
		"]," + "\n" +
		"\"includePlatforms\": []," + "\n" +
		"\"excludePlatforms\": []," + "\n" +
		"\"allowUnsafeCode\": false," + "\n" +
		"\"overrideReferences\": false," + "\n" +
		"\"precompiledReferences\": []," + "\n" +
		"\"autoReferenced\": true," + "\n" +
		"\"defineConstraints\": []," + "\n" +
		"\"versionDefines\": []" + "\n" +
		"}";
		


	[MenuItem("Tools/Databox/Enable Addressables support")]
	public static void EnableAddressablesSupport()
	{
		
		string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup ( EditorUserBuildSettings.selectedBuildTargetGroup );
		List<string> allDefines = definesString.Split ( ';' ).ToList ();
			
		// check if define symbol is already added
		bool _exists = false;
		for (int i = 0; i < allDefines.Count; i ++)
		{
			if (allDefines[i].Contains(defineSymbolsAddressables[0]))
			{
				_exists = true;
			}
		}
			
		if (!_exists)
		{		

			allDefines.AddRange ( defineSymbolsAddressables.Except ( allDefines ) );
				
			PlayerSettings.SetScriptingDefineSymbolsForGroup 
			(
				EditorUserBuildSettings.selectedBuildTargetGroup,
				string.Join ( ";", allDefines.ToArray () ) 
			);
		}
		
		
		CreateAssemblyDefinitionFile();
	}
	
	static void CreateAssemblyDefinitionFile()
	{
		bool _assemblyAddressablesDefinitions = false;
	
		Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies();
		for(int i = 0; i < playerAssemblies.Length; i ++)
		{
			
			if (playerAssemblies[i].name == "Databox")
			{
				for (int d = 0; d < playerAssemblies[i].assemblyReferences.Length; d ++)
				{
					if (playerAssemblies[i].assemblyReferences[d].name == "Unity.Addressables")
					{
						_assemblyAddressablesDefinitions = true;
					}
		
					if (playerAssemblies[i].assemblyReferences[d].name == "Unity.Addressables.Editor")
					{
						_assemblyAddressablesDefinitions = true;
					}
		
					if (playerAssemblies[i].assemblyReferences[d].name == "Unity.ResourceManager")
					{
						_assemblyAddressablesDefinitions = true;
					}
				}
			}
		}
		
		if (!_assemblyAddressablesDefinitions)
		{
			// Modify Databox Assembly Definition
			var _res = System.IO.Directory.EnumerateFiles("Assets/", "Databox.asmdef", System.IO.SearchOption.AllDirectories);
				
			var _path = "";
				
			var _found = _res.FirstOrDefault();
			if (!string.IsNullOrEmpty(_found))
			{
				_path = _found.Replace("\\", "/");
			
				using (System.IO.StreamWriter sw = System.IO.File.CreateText(_path))
				{
					sw.Write(assemblyDefinition);
				}
				
				AssetDatabase.ImportAsset(_path);			
			}
		}
	}
}
#endif