using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using Databox;
using Databox.Ed;

namespace Databox.Ed
{
	public class DataboxObjectEditorWindow : EditorWindow
	{
		
		public class DataboxObjectsData
		{
			public string path;
			public DataboxObject data;
		}
		
		public Dictionary<string, List<DataboxObject>> databoxObjects = new Dictionary<string, List<DataboxObject>>();
		
		public string selectedDirectory;
		public int selectedIndex;
		public DataboxObject selectedObject;
		
		public bool collapsedDirectoryList;
		public bool collapsedObjectsList;
		
		public float hierarchyWidth = 200;
		
		private Vector2 scroll;
		EditorGUIHierarchyView hierarchyView = new EditorGUIHierarchyView();
		
		public DataboxObject lastSelectedDataboxObject;
		
		[SerializeField]
		static AutocompleteSearchField autocompleteSearchField;
		
		string searchString;
		
		bool isDragging = false;
		Event currentEvent;
		
		[MenuItem("Tools/Databox/Editor", false, 1)]
		static void Init()
		{
			DataboxObjectEditorWindow window = (DataboxObjectEditorWindow)EditorWindow.GetWindow(typeof(DataboxObjectEditorWindow));
			window.Show();
		}
		
		void InitSearchfield()
		{
			if (autocompleteSearchField == null) autocompleteSearchField = new AutocompleteSearchField();
			autocompleteSearchField.onInputChanged = OnInputChanged;
			autocompleteSearchField.onConfirm = OnConfirm;
			
			
		}
		
		
		void OnInputChanged(string _searchString)
		{
			autocompleteSearchField.ClearResults();
			searchString = _searchString;
		}
		
		void OnConfirm(string result)
		{
			autocompleteSearchField.ClearResults();
			autocompleteSearchField.showResults = false;
			searchString = result;
		}
		
		
		void OnGUI()
		{
			
			currentEvent = Event.current;
			
			if (databoxObjects.Keys.Count == 0)
			{
				GetAllInstances<DataboxObject>();
			}
			
			using (new GUILayout.HorizontalScope("Toolbar"))
			{
				if (GUILayout.Button("Refresh", "ToolbarButton"))
				{
						GetAllInstances<DataboxObject>();
				}
				
				GUILayout.FlexibleSpace();
			}
		
			
			if (databoxObjects == null || databoxObjects.Keys.Count == 0)
			{
				EditorGUILayout.HelpBox("Please reload", MessageType.Info);
				return;
			}
				
			try{
			using (new GUILayout.HorizontalScope())
			{
				
				
				using (new GUILayout.VerticalScope("TextArea", GUILayout.Width(hierarchyWidth)))
				{
					
					InitSearchfield();
					
					GUILayout.Label("");
					var _rect = GUILayoutUtility.GetLastRect();
					
				
				hierarchyView.BeginHierarchyView();
				
				foreach (var _dir in databoxObjects.Keys)
				{
					if (string.IsNullOrEmpty(searchString))
					{
						hierarchyView.BeginNode(_dir);	
					}
						
					for(int i = 0; i < databoxObjects[_dir].Count; i++)
					{
						if (databoxObjects[_dir][i] == null)
						{
							databoxObjects[_dir].RemoveAt(i);
							continue;	
						}
						
						if (!string.IsNullOrEmpty(searchString))
						{
							if (databoxObjects[_dir][i].name.ToLower().Contains(searchString.ToLower()))
							{
								
								bool isSelected = hierarchyView.Node(databoxObjects[_dir][i].name, databoxObjects[_dir][i]);
								if (isSelected)
								{
								selectedObject = databoxObjects[_dir][i];
								selectedDirectory = _dir;
									selectedIndex = i;
								
								
									CheckForUnsavedChanges(selectedObject);
								}
							}
						}
						else
						{ 
							bool isSelected = hierarchyView.Node(databoxObjects[_dir][i].name, databoxObjects[_dir][i]);
							//hierarchyView.EndNode();
							if (isSelected)
							{
								selectedDirectory = _dir;
								selectedIndex = i;
								selectedObject = databoxObjects[_dir][i];
								
								CheckForUnsavedChanges(selectedObject);
								//selectedObject.LoadDatabase();
							}
						}
					}
					
					if (string.IsNullOrEmpty(searchString))
					{
						hierarchyView.EndNode();
					}
				}
				
				
					
				
					hierarchyView.EndHierarchyView();
				
					Repaint();
					autocompleteSearchField.OnGUI(_rect);
				
			
					
				}
				
				// Split drag
				///////////////
				GUILayout.Box("", GUILayout.Width(6), GUILayout.Height(Screen.height));
				var buttonRect = GUILayoutUtility.GetLastRect();
				buttonRect = new Rect(buttonRect.x-5, buttonRect.y, buttonRect.width + 10, buttonRect.height);
				EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.ResizeHorizontal);
				
				
				
				if (currentEvent.type == EventType.MouseDown)
				{
					if (buttonRect.Contains(Event.current.mousePosition))
					{
						isDragging = true;
					}
				}
				
			
				if (isDragging && currentEvent.type == EventType.MouseDrag)
				{
					hierarchyWidth += currentEvent.delta.x;
					
					if (hierarchyWidth <= 5)
					{
						hierarchyWidth = 5;
					}
					if (hierarchyWidth >= position.width)
					{
						hierarchyWidth = position.width - 50;
					}
				}

				
				if (currentEvent.type == EventType.MouseUp)
				{
					isDragging = false;
				}
				///////////////
				
				using (new GUILayout.VerticalScope("TextArea"))
				{
					if (selectedObject != null)
					{
						
						GUILayout.Label(selectedDirectory + System.IO.Path.DirectorySeparatorChar + selectedObject.name, "boldLabel");
						
						GUILayout.Space(2);
						DataboxEditor.DrawEditor(selectedObject);
				
					}
				}
			}
			
			}catch
			{
				
			}
		}
	
		void CheckForUnsavedChanges(DataboxObject _databoxObject)
		{
			
				if (lastSelectedDataboxObject != null)
				{
					if (_databoxObject != lastSelectedDataboxObject)
					{
						if (lastSelectedDataboxObject.errors == DataboxObject.ErrorType.UnsavedChanges)
						{
							if (EditorUtility.DisplayDialog("Unsaved changes", "Save changes?", "Yes", "No"))
							{
								lastSelectedDataboxObject.SaveDatabase();
							}
							else
							{
								lastSelectedDataboxObject = _databoxObject;
							}
						}
						lastSelectedDataboxObject = _databoxObject;
					}
				}
				else
				{
					lastSelectedDataboxObject = _databoxObject;
				}
			
				
		}
	
		void GetAllInstances<T>() where T : ScriptableObject
		{
			string[] guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);
			T[] a = new T[guids.Length];
			databoxObjects = new Dictionary<string, List<DataboxObject>>();
			
			for(int i =0;i<guids.Length;i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
				
				var _dirName = Path.GetDirectoryName(path);
				
				if (databoxObjects.ContainsKey(_dirName))
				{
					databoxObjects[_dirName].Add(a[i] as DataboxObject);
				}
				else
				{
					var _ls = new List<DataboxObject>();
					_ls.Add(a[i]as DataboxObject);
					databoxObjects.Add(_dirName, _ls);
				}
			}
	 
		}
	}
}
