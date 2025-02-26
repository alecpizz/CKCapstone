using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using Databox.Utils;

namespace Databox.Ed
{
	[CustomEditor(typeof(DataboxSchemes))]
	public class DataboxSchemesEditor : Editor
	{
		
		List<string> existingTypeNames = new List<string>();
		List<string> existingTypes = new List<string>();
		
		public override void OnInspectorGUI()
		{ 
			DataboxSchemes dbSchemes = (DataboxSchemes)target;
			
			
			if (existingTypeNames == null || existingTypeNames.Count == 0)
			{
				CollectAllTypes(); 
			}
			
			// check if type index has been changed due to new types being added
			for (int s = 0; s < dbSchemes.schemes.Count; s ++)
			{				
				for (int u = 0; u < dbSchemes.schemes[s].schemeTypes.Count; u ++)
				{
					if (dbSchemes.schemes[s].schemeTypes[u].type != existingTypes[dbSchemes.schemes[s].schemeTypes[u].typeIndex])
					{
						CollectAllTypes(); 
						
						for (int e = 0; e < existingTypes.Count; e ++)
						{
							if (dbSchemes.schemes[s].schemeTypes[u].type == existingTypes[e])
							{
								dbSchemes.schemes[s].schemeTypes[u].typeIndex = e;
							}
						}
					}
				}	
			}
				
			using (new GUILayout.VerticalScope("Window"))
			{
			
				if (GUILayout.Button("Add New Scheme"))
				{
					dbSchemes.schemes.Add(new DataboxSchemes.Schemes("NEW SCHEME"));
				}
				
				try
				{
					for (int s = 0; s < dbSchemes.schemes.Count; s ++)
					{
						
						//for (int u = 0; u < dbSchemes.schemes[s].schemeTypes.Count; u ++)
						//{
						//	// check if type index has been changed due to new types being added
						//	for (int e = 0; e < existingTypeNames.Count; e ++)
						//	{
						//		if (dbSchemes.schemes[s].schemeTypes[u].type == existingTypeNames[e])
						//		{
						//			if (dbSchemes.schemes[s].schemeTypes[u].typeIndex != e)
						//			{
						//				// update index
						//				Debug.Log("update index " + "old: " + dbSchemes.schemes[s].schemeTypes[u].typeIndex + " new: " + e);
						//				dbSchemes.schemes[s].schemeTypes[u].typeIndex = e;
						//			}
						//		}
						//	}
						//}				
										
										
										
						using (new GUILayout.VerticalScope("Box"))
						{
							using (new GUILayout.HorizontalScope("Toolbar"))
							{
								GUILayout.Space(7);
								dbSchemes.schemes[s].foldout = EditorGUILayout.Foldout(dbSchemes.schemes[s].foldout, dbSchemes.schemes[s].schemeName, true);
								
								//GUILayout.FlexibleSpace();
								if (GUILayout.Button("x", "ToolbarButton", GUILayout.Width(20)))
								{
									dbSchemes.schemes.RemoveAt(s);
								}
							}
							
							if (dbSchemes.schemes[s].foldout)
							{
								dbSchemes.schemes[s].schemeName = EditorGUILayout.TextField("Scheme name:", dbSchemes.schemes[s].schemeName);
								
								if (GUILayout.Button("Add New Type"))
								{
									dbSchemes.schemes[s].schemeTypes.Add(new DataboxSchemes.Schemes.SchemeType());
								}
								
							
								for (int u = 0; u < dbSchemes.schemes[s].schemeTypes.Count; u ++)
								{
									using (new GUILayout.VerticalScope("Box"))
									{
										var _nameAlreadyExists = false;
										using (new GUILayout.HorizontalScope())
										{
											
											var _valueName = EditorGUILayout.TextField("Value name:", dbSchemes.schemes[s].schemeTypes[u].name);
											
											for (int n = 0; n < dbSchemes.schemes[s].schemeTypes.Count; n ++ )
											{
												if (dbSchemes.schemes[s].schemeTypes[n].name == _valueName && n != u)
												{
													_nameAlreadyExists = true;	
												}
											}
											
											
											
											dbSchemes.schemes[s].schemeTypes[u].name = _valueName;
											
											
											if (GUILayout.Button("x", "miniButton", GUILayout.Width(20)))
											{
												dbSchemes.schemes[s].schemeTypes.RemoveAt(u);
											}
										}
										
										if (_nameAlreadyExists)
										{
												
											EditorGUILayout.HelpBox("Name already exists, please use a unique name", MessageType.Warning);
												
										}
										
									
										dbSchemes.schemes[s].schemeTypes[u].typeIndex = EditorGUILayout.Popup("Type:", dbSchemes.schemes[s].schemeTypes[u].typeIndex, existingTypeNames.ToArray());
										
										var _newType = existingTypes[dbSchemes.schemes[s].schemeTypes[u].typeIndex];
										dbSchemes.schemes[s].schemeTypes[u].type = _newType;
										
									}
								} 
							
							}
						}
					}
				}
				catch{}
			}
			
			EditorUtility.SetDirty(target);
	
		}
		
		void CollectAllTypes()
		{
			
			existingTypeNames = new List<string>();
			existingTypes = new List<string>();
			
			var types = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(DataboxType));
			foreach(var type in types)
			{
				var _attribute = type.GetCustomAttributes(typeof(DataboxTypeAttribute), false);
				if (_attribute.Length > 0)
				{
					var _targetAttribute = _attribute.First() as DataboxTypeAttribute;	
					existingTypeNames.Add(_targetAttribute.Name);
				}
				else
				{
					existingTypeNames.Add(type.FullName.ToString());
				}
				
				existingTypes.Add(type.FullName.ToString());
			}

			//for (int i = 0; i < existingTypeNames.Count; i ++)
			//{
			//	Debug.Log(existingTypeNames[i]);
			//}
		}
		
	}
}
