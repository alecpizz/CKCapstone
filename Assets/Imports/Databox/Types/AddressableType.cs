
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!! USE THE UNITY PACKAGE MANAGER TO DOWNLOAD THE ADDRESSABLES PACKAGE BEFORE USING THIS TYPE
//!!
//!!	1. Enable the Addressables support by selecting Tools / Databox / Enable Addressables support
//!!	2. Uncomment the code in this file
//!!
//!! Pleas note: Addressables are still in Beta. 
//!! Therefore it's possible that this script might spawn error messages.
//!!
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


//using System.Collections;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//using UnityEngine;
//#if DATABOX_ADDRESSABLES
//using UnityEngine.AddressableAssets;
//#endif
//#if UNITY_EDITOR
//#if DATABOX_ADDRESSABLES
//using UnityEditor.AddressableAssets;
//using UnityEditor.AddressableAssets.Settings;
//#endif
//using UnityEditor;
//#endif
//#if DATABOX_ADDRESSABLES
//using UnityEngine.ResourceManagement;
//using UnityEngine.ResourceManagement.AsyncOperations;
//#endif

//using System.IO;
//using Databox;
//using Databox.Utils;


//[System.Serializable]
//public class AddressableType : DataboxType
//{
//	public string assetPath;

//	Rect dropdownRect;
//	float popupWidth;
//	bool isDragging;
	
//	#if DATABOX_ADDRESSABLES
//	public UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> Load()
//	{
//		try
//		{
//			var o = Addressables.LoadAssetAsync<GameObject>(assetPath);
//			return o;
//		}
//		catch
//		{ 
//			return new UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject>();
//		}
//	}

//	public IEnumerator LoadAddressableAssetIE<T>(System.Action<T> OnLoadingComplete)
//	{
//		AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetPath);
			
//		yield return handle;
			
//		if (handle.Status == AsyncOperationStatus.Succeeded)
//		{
//			var _mat = (T) handle.Result;
//			OnLoadingComplete(_mat);
//		}
//	}
//	#endif

//	public override void DrawEditor()
//	{
		
//		#if UNITY_EDITOR
//		using (new GUILayout.HorizontalScope("Box"))
//		{
		
//		#if DATABOX_ADDRESSABLES
//			var allAssets = new List<AddressableAssetEntry>();

//			try
//			{
//				using (new GUILayout.VerticalScope())
//				{
//					DropAreaGUI();
					
//					GUILayout.Label("Or select from dropdown:");
					
//					GUILayout.Label("",GUILayout.Height(1));
//					dropdownRect = GUILayoutUtility.GetLastRect();
				
//					if (EditorGUILayout.DropdownButton(new GUIContent(assetPath), FocusType.Keyboard, GUILayout.ExpandWidth(true)))
//					{	
//						AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(allAssets, false);
//						PopupWindow.Show(dropdownRect, new PopupContent(allAssets, this));
//					}
//					var x = GUILayoutUtility.GetLastRect();
//					if (x.width > 1)
//					{
//						popupWidth = x.width;
//					}			
					
//					GUILayout.Label(assetPath, "boldLabel");
//				}
//			}
//			catch
//			{
//				EditorGUILayout.HelpBox("No addressables found. Please make sure to have at least one prefab as addressable.", MessageType.Info);
//			}
			
//		#else
		
//			using (new GUILayout.VerticalScope())
//			{
//				EditorGUILayout.HelpBox("Before you can use the addressables type, please make sure you have installed the Addressables package from the package manager.", MessageType.Info);
				
//				if (GUILayout.Button("Enable Addressables integration", GUILayout.Height(40)))
//				{
//					DataboxSetup.EnableAddressablesSupport();
//				}
//			}
		
//		#endif
		
//		}
//		#endif
//	}
	

//	#if UNITY_EDITOR && DATABOX_ADDRESSABLES
//	public class PopupContent : PopupWindowContent
//	{
//		AddressableType value;
//		List<AddressableAssetEntry> assets = new List<AddressableAssetEntry>();
//		string searchString = "";

//		public override Vector2 GetWindowSize()
//		{
//			return new Vector2(value.popupWidth, ((assets.Count + 1) * 22) + 60);
//		}

//		public override void OnGUI(Rect rect)
//		{	
//			GUILayout.Label("Addressables", EditorStyles.boldLabel);
			
//			using (new GUILayout.HorizontalScope())
//			{
//				GUI.SetNextControlName ("FilterAddressables");
//				searchString = GUILayout.TextField(searchString, "SearchTextField");
						
//				if (GUILayout.Button("", GUI.skin.FindStyle("SearchCancelButton")))
//				{
//					searchString = "";
//				}
//			}
			
//			if (GUILayout.Button("none"))
//			{
//				value.assetPath = "";
//				editorWindow.Close();
//			}
			
//			foreach(var entry in assets)
//			{
//				if (entry.address.ToLower().Contains(searchString.ToLower()) || string.IsNullOrEmpty(searchString))
//				{
//					using (new GUILayout.HorizontalScope())
//					{
//						var _icon = AssetDatabase.GetCachedIcon(entry.AssetPath) as Texture2D;
//						GUILayout.Label(_icon, GUILayout.Height(20));
//						if (GUILayout.Button(entry.address.ToString()))
//						{
//							//assetPa
//							value.assetPath = entry.address;
//							editorWindow.Close();
//						}
//					}
//				}
//			}
			
//		}

//		public override void OnOpen()
//		{
//			//Debug.Log("Popup opened: " + this);
//			EditorGUI.FocusTextInControl ("FilterAddressables");
//		}

//		public override void OnClose()
//		{
//			//Debug.Log("Popup closed: " + this);
//		}
		
//		public PopupContent(List<AddressableAssetEntry> _assets, AddressableType _value)
//		{
//			assets = new List<AddressableAssetEntry>(_assets);
//			//assets = _assets;
//			value = _value;
//		}
//	}
	
//	public void DropAreaGUI()
//	{
		
//		Event _evt = Event.current;
//		Rect _dropArea = GUILayoutUtility.GetRect(120f, 16f);
			
//		if (isDragging)
//		{
//			GUI.contentColor = Color.green;
//		}
//		else
//		{
//			GUI.contentColor = Color.white;
//		}
//		GUI.Box (_dropArea, "Drag object here", "ObjectField");
//		GUI.contentColor = Color.white;
//		switch (_evt.type)
//		{
//		case EventType.DragUpdated:
//		case EventType.DragPerform:
//			if (!_dropArea.Contains(_evt.mousePosition))
//			{
//				isDragging = false;
//				return;
//			}
//			else
//			{
//				isDragging = true;
//				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						
//				if (_evt.type == EventType.DragPerform)
//				{
//					isDragging = false;
//					DragAndDrop.AcceptDrag();
//					foreach (Object _dobj in DragAndDrop.objectReferences)
//					{

					
//						string path = AssetDatabase.GetAssetPath(_dobj);
//						string guid = AssetDatabase.AssetPathToGUID(path);
//						var assetEntry = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid);
							
//						if (assetEntry == null)
//						{
//							Debug.LogError("Object is not an Addressable asset");
//						}
//						else
//						{
//							assetPath = assetEntry.address;
//						}
						
							
//					}
//				}
						
//			}
//			break;
//		}
//	}
//	#endif

//}
