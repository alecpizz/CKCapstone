using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Databox;
using Databox.CSVReader;

public class DataboxCSVConverter
{
	static string ignoreKey = "DB_IGNORE";
	static string fieldNameKey = "DB_FIELD_NAMES";
	static string fieldTypeKey = "DB_FIELD_TYPES";
	
	public static bool firstTimeReplace = true;
	
	[System.Serializable]
	public class Entry
	{
		public string entryName = "";
		public List<string> fields = new List<string>();
		public List<string> types = new List<string>();
		public List<string> values = new List<string>();
		
		public Entry(string _entryName)
		{
			entryName = _entryName;
		}
	}

	
	public static void ConvertCSV(string _input, out List<Entry> entries)
	{

		entries = new List<Entry>();
		
		
		var _output = DataboxCSVReader.SplitCSV(_input);
	
		var _fieldTypesIndex = 0;
		var _fieldNamesIndex = 0;
		
		foreach (var row in _output.Keys)
		{
			for(int i = 0; i < _output[row].Count; i ++)
			{
				
				if (_output[row][i] == fieldTypeKey)
				{
					_fieldTypesIndex = i;
				}
				
				if (_output[row][i] == fieldNameKey)
				{
					_fieldNamesIndex = i;
				}
				
				//// Entry names
				if (row == 0 && i != _fieldNamesIndex && i != _fieldTypesIndex)
				{
				
					var _entryIndex = i;
				
					if (!string.IsNullOrEmpty(_output[row][i]) && _output[row][i] != ignoreKey)
					{
						entries.Add(new Entry(_output[row][i]));
					
						foreach (var row2 in _output.Keys)
						{
							for (int i2 = 0; i2 < _output[row2].Count; i2 ++)
							{
								// field names
								if (row2 > 0 && i2 == _fieldNamesIndex)
								{
									if (!string.IsNullOrEmpty(_output[row2][i2]))
									{
										entries[entries.Count-1].fields.Add(_output[row2][i2]);
									}
								}
								
								// field types
								if (row2 > 0 && i2 == _fieldTypesIndex)
								{
									if (!string.IsNullOrEmpty(_output[row2][i2]))
									{
										entries[entries.Count-1].types.Add(_output[row2][i2]);
									}
								}
								// values
								if (row2 > 0 && i2 == _entryIndex && i2 != _fieldTypesIndex && i2 != _fieldNamesIndex)
								{
									entries[entries.Count-1].values.Add(_output[row2][i2]);
								}
							}
						}
						
					}
				}
			}
		}
		
		// cleanup
		for (int e = 0; e < entries.Count; e ++)
		{
			for (int f = 0; f < entries[e].fields.Count; f ++)
			{
				if (entries[e].fields[f] == ignoreKey)
				{
					entries[e].types[f] = ignoreKey;
					entries[e].values[f] = ignoreKey;
					entries[e].fields[f] = ignoreKey;
				}
			}
			
			for (int v = 0; v < entries[e].values.Count; v ++)
			{
				if (string.IsNullOrEmpty(entries[e].values[v]))
				{
					if (v < entries[e].types.Count)
					{
						entries[e].types[v] = ignoreKey;
					}

					if (v < entries[e].fields.Count)
					{
						entries[e].fields[v] = ignoreKey;
					}
					
					if (v < entries[e].values.Count)
					{
						entries[e].values[v] = ignoreKey;
					}
				}
			}
		}
		
		// remove all entries which contains the ignore key
		for (int e = 0; e < entries.Count; e ++)
		{
			for (int v = entries[e].values.Count - 1; v >= 0; v --)
			{
				if (entries[e].values[v].Contains(ignoreKey))
				{
					entries[e].values.RemoveAt(v);
				}
			}

			for (int t = entries[e].types.Count - 1; t >= 0 ; t --)
			{
				if (entries[e].types[t].Contains(ignoreKey))
				{
				
					entries[e].types.RemoveAt(t);
				}
			}

			for (int f = entries[e].fields.Count - 1; f >= 0; f --)
			{
				if (entries[e].fields[f].Contains(ignoreKey))
				{
					entries[e].fields.RemoveAt(f);
				}
			}
		}
	}
	
	#if !DATABOX_LOCALIZATION
	public static void AppendToDB(DataboxObject _database, string _tableName, List<Entry> _entries)
	{
		for (int e = 0; e < _entries.Count; e ++)
		{
			for (int a = 0; a < _entries[e].values.Count; a ++)
			{

				Dictionary<string, System.Type> _collectedTypes = new Dictionary<string, System.Type>();
				
				foreach (System.Reflection.Assembly b in System.AppDomain.CurrentDomain.GetAssemblies())
				{
					System.Type[] assemblyTypes = b.GetTypes();
					for (int j = 0; j < assemblyTypes.Length; j++)
					{
						var _str = _entries[e].types[a].Trim();
						if (assemblyTypes[j].Name == _str)
						{
								
							string _namespace = "databox";
							if (!string.IsNullOrEmpty(assemblyTypes[j].Namespace))
							{
								_namespace = assemblyTypes[j].Namespace;
							}
								
							_collectedTypes.Add(_namespace, assemblyTypes[j]);
								
						}
					}
				}
					
				if (_collectedTypes.Count == 0)
				{
					Debug.LogError("Databox - type: " + _entries[e].types[a] + " does not exist in project");
					return;
				}
					
					
				// There could be more than one type with the same name in different namespaces
				// In this case we use the most obvious one which is in no namespace
				// Occurs with ResourceType. Which conflicts with:
				//.....
				// ResourceType in namespace System.Security.AccessControl
				// ResourceType in namespace Mono.Cecil
				//.....
				System.Type _foundType = null;
				if (_collectedTypes.Count > 1)
				{
					_foundType = _collectedTypes["databox"];
				}
				else
				{
					_foundType = _collectedTypes.FirstOrDefault().Value;
				}
					
					
				var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
					
				// Convert csv string value to DataboxType value
				_instance.Convert(_entries[e].values[a]);

				_database.AddDataCSV(_tableName, _entries[e].entryName, _entries[e].fields[a], _instance as DataboxType);
			}
		}
	}
	
	#else
	
	public static void AppendToDB(DataboxObject _database, string _tableName, List<Entry> _entries, bool _isLocalizationTable)
	{
		// Add entries
		if (_isLocalizationTable)
		{
			for (int e = 0; e < _entries.Count; e ++)
			{

				System.Type _foundType = null;
									
				foreach (System.Reflection.Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
				{
					System.Type[] assemblyTypes = a.GetTypes();
					for (int j = 0; j < assemblyTypes.Length; j++)
					{
						if (assemblyTypes[j].Name == "LocalizationType")
						{
							_foundType = assemblyTypes[j];
						}
					}
				}
				
				
				var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
					
				// Convert csv string value to DataboxType value
				_instance.ConvertRow(_entries[e]);	
				_database.AddDataCSV(_tableName, _entries[e].entryName,  _entries[e].entryName, _instance as DataboxType);
			
			}
			
		}
		else
		{
			for (int e = 0; e < _entries.Count; e ++)
			{
				for (int a = 0; a < _entries[e].values.Count; a ++)
				{

					Dictionary<string, System.Type> _collectedTypes = new Dictionary<string, System.Type>();

					foreach (System.Reflection.Assembly b in System.AppDomain.CurrentDomain.GetAssemblies())
					{
						System.Type[] assemblyTypes = b.GetTypes();
						for (int j = 0; j < assemblyTypes.Length; j++)
						{
							var _str = _entries[e].types[a].Trim();
							if (assemblyTypes[j].Name == _str)
							{
								
								string _namespace = "databox";
								if (!string.IsNullOrEmpty(assemblyTypes[j].Namespace))
								{
									_namespace = assemblyTypes[j].Namespace;
								}
								
								_collectedTypes.Add(_namespace, assemblyTypes[j]);
								
							}
						}
					}
					
					if (_collectedTypes.Count == 0)
					{
						Debug.LogError("Databox - type: " + _entries[e].types[a] + " does not exist in project");
						return;
					}
					
					
					// There could be more than one type with the same name in different namespaces
					// In this case we use the most obvious one which is in no namespace
					// Occurs with ResourceType. Which conflicts with:
					//.....
					// ResourceType in namespace System.Security.AccessControl
					// ResourceType in namespace Mono.Cecil
					//.....
					System.Type _foundType = null;
					if (_collectedTypes.Count > 1)
					{
						_foundType = _collectedTypes["databox"];
					}
					else
					{
						_foundType = _collectedTypes.FirstOrDefault().Value;
					}
					
					
					var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
					
					// Convert csv string value to DataboxType value
					_instance.Convert(_entries[e].values[a]);

					_database.AddDataCSV(_tableName, _entries[e].entryName, _entries[e].fields[a], _instance as DataboxType);
				}
			}
		}
	}	
	#endif
	
	#if !DATABOX_LOCALIZATION
	public static void ReplaceDB(DataboxObject _database, string _tableName, List<Entry> _entries)
	{
		if (firstTimeReplace)
		{
			_database.DB = new Databox.Dictionary.OrderedDictionary<string, DataboxObject.Database>();
			firstTimeReplace = false;
		}
		
		// Add entries
		for (int e = 0; e < _entries.Count; e ++)
		{
			for (int a = 0; a < _entries[e].values.Count; a ++)
			{
				Dictionary<string, System.Type> _collectedTypes = new Dictionary<string, System.Type>();
				
				foreach (System.Reflection.Assembly b in System.AppDomain.CurrentDomain.GetAssemblies())
				{
					System.Type[] assemblyTypes = b.GetTypes();
					for (int j = 0; j < assemblyTypes.Length; j++)
					{
						var _str = _entries[e].types[a].Trim();
						if (assemblyTypes[j].Name == _str)
						{
						
							string _namespace = "databox";
							if (!string.IsNullOrEmpty(assemblyTypes[j].Namespace))
							{
								_namespace = assemblyTypes[j].Namespace;
							}
								
							_collectedTypes.Add(_namespace, assemblyTypes[j]);
								
						}
					}
				}
					
				if (_collectedTypes.Count == 0)
				{
					Debug.LogError("Databox - type: " + _entries[e].types[a] + " does not exist in project");
					return;
				}

				// There could be more than one type with the same name in different namespaces
				// In this case we use the most obvious one which is in no namespace
				// Occurs with ResourceType. Which conflicts with:
				//.....
				// ResourceType in namespace System.Security.AccessControl
				// ResourceType in namespace Mono.Cecil
				//.....
				System.Type _foundType = null;
				if (_collectedTypes.Count > 1)
				{
					_foundType = _collectedTypes["databox"];
				}
				else
				{
					_foundType = _collectedTypes.FirstOrDefault().Value;
				}
					

				var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
					
				// Convert csv string value to DataboxType value
				_instance.Convert(_entries[e].values[a]);
					
				_database.AddDataCSV(_tableName, _entries[e].entryName, _entries[e].fields[a], _instance as DataboxType);
			}
		}
	}
	
	#else
	
	public static void ReplaceDB(DataboxObject _database, string _tableName, List<Entry> _entries, bool _isLocalizationTable)
	{
		if (firstTimeReplace)
		{
			_database.DB = new Databox.Dictionary.OrderedDictionary<string, DataboxObject.Database>();
			firstTimeReplace = false;
		}
		
		if (_isLocalizationTable)
		{
			bool _isLanguageType = false;
			bool _isLocalizationType = false;
			
			
			
			if (_entries[0].types[0].Contains("LanguagesType"))
			{
				_isLanguageType = true;
			}
			else if (_entries[0].types[0].Contains("LocalizationType"))
			{
				_isLocalizationType = true;
			}
			
		
			// LANGUAGE TYPE
			if (_isLanguageType)
			{
				for (int e = 0; e < _entries.Count; e ++)
				{
					System.Type _foundType = null;
											
					foreach (System.Reflection.Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
					{
						System.Type[] assemblyTypes = a.GetTypes();
						for (int j = 0; j < assemblyTypes.Length; j++)
						{
							if (assemblyTypes[j].Name == "LanguagesType")
							{
								_foundType = assemblyTypes[j];
							}
						}
					}
						
					var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
					
					_instance.ConvertRow(_entries[e]);
					
					_database.AddDataCSV(_tableName, _entries[e].entryName, _entries[e].entryName, _instance as DataboxType);
				}
			}
				
			
			// LOCALIZATION TYPE
			if (_isLocalizationType)
			{
				for (int e = 0; e < _entries.Count; e ++)
				{

					System.Type _foundType = null;
										
					foreach (System.Reflection.Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
					{
						System.Type[] assemblyTypes = a.GetTypes();
						for (int j = 0; j < assemblyTypes.Length; j++)
						{
							if (assemblyTypes[j].Name == "LocalizationType")
							{
								_foundType = assemblyTypes[j];
							}
						}
					}
					
					var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
		
					// Convert csv string value to DataboxType value
					_instance.ConvertRow(_entries[e]);
						
					_database.AddDataCSV(_tableName, _entries[e].entryName, _entries[e].entryName, _instance as DataboxType);
				}
			}
		}
		else
		{
			// Add entries
			for (int e = 0; e < _entries.Count; e ++)
			{
				for (int a = 0; a < _entries[e].values.Count; a ++)
				{
					Dictionary<string, System.Type> _collectedTypes = new Dictionary<string, System.Type>();
				
					foreach (System.Reflection.Assembly b in System.AppDomain.CurrentDomain.GetAssemblies())
					{
						System.Type[] assemblyTypes = b.GetTypes();
						for (int j = 0; j < assemblyTypes.Length; j++)
						{
							var _str = _entries[e].types[a].Trim();
							if (assemblyTypes[j].Name == _str)
							{

								string _namespace = "databox";
								if (!string.IsNullOrEmpty(assemblyTypes[j].Namespace))
								{
									_namespace = assemblyTypes[j].Namespace;
								}
								
								_collectedTypes.Add(_namespace, assemblyTypes[j]);
								
							}
						}
					}
					
					if (_collectedTypes.Count == 0)
					{
						Debug.LogError("Databox - type: " + _entries[e].types[a] + " does not exist in project");
						return;
					}

					// There could be more than one type with the same name in different namespaces
					// In this case we use the most obvious one which is in no namespace
					// Occurs with ResourceType. Which conflicts with:
					//.....
					// ResourceType in namespace System.Security.AccessControl
					// ResourceType in namespace Mono.Cecil
					//.....
					System.Type _foundType = null;
					if (_collectedTypes.Count > 1)
					{
						_foundType = _collectedTypes["databox"];
					}
					else
					{
						_foundType = _collectedTypes.FirstOrDefault().Value;
					}
					

					var _instance = System.Activator.CreateInstance(_foundType) as DataboxType;
					
					// Convert csv string value to DataboxType value
					_instance.Convert(_entries[e].values[a]);
					
					_database.AddDataCSV(_tableName, _entries[e].entryName, _entries[e].fields[a], _instance as DataboxType);
				}
			}
		}
	}
	#endif
}
