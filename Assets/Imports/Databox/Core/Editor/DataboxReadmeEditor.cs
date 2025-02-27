using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace Databox.Ed
{
	[CustomEditor(typeof (DataboxReadme))]
	public class DataboxReadmeEditor : Editor
	{
		
		DataboxReadme readme;
		
		GUIStyle LinkStyle { get { return m_LinkStyle; } }
		[SerializeField] GUIStyle m_LinkStyle;
		
		GUIStyle TitleStyle { get { return m_TitleStyle; } }
		[SerializeField] GUIStyle m_TitleStyle;
		
		GUIStyle BodyStyle { get { return m_BodyStyle; } }
		[SerializeField] GUIStyle m_BodyStyle;
		
		Texture2D logo;
		Vector2 scrollPosition;
		
		
		static string GetRelativePath()
		{
			string[] res = System.IO.Directory.GetFiles("Assets/", "DataboxObject.cs", System.IO.SearchOption.AllDirectories);
			if (res.Length == 0)
			{
				//Debug.LogError("error");
				return null;
			}
			string path = res[0].Replace("DataboxObject.cs", "").Replace("\\", "/");
			path = path.Replace("/Core", "");
				
			return path;
		}
			
			
			
		void OnEnable()
		{
			readme = (DataboxReadme)target;
			
			
			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;
			m_BodyStyle.richText = true;
			
			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 22;
	
			m_LinkStyle = new GUIStyle(m_BodyStyle);
			m_LinkStyle.wordWrap = false;
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color (0x00/255f, 0x78/255f, 0xDA/255f, 1f);
			m_LinkStyle.stretchWidth = false;
			
			
			var _path = System.IO.Path.Combine(GetRelativePath(), "GUI");
			logo = (Texture2D)AssetDatabase.LoadAssetAtPath(_path + "/" + "logoheader.png", typeof(Texture2D));
	
			LoadChangelog();	
		}
		
		
		
		public override void OnInspectorGUI()
		{
			GUILayout.Label(logo);
			
			GUILayout.Label("Readme", m_TitleStyle);
			GUILayout.Label(readme.version, m_BodyStyle);
			GUILayout.Label(readme.text, m_BodyStyle);
			
			
	
			
			if (GUILayout.Button("Documentation"))
			{
				Application.OpenURL(readme.documentationLink);
			}
		
		
			
			if (GUILayout.Button("Website"))
			{
				Application.OpenURL(readme.websiteLink);
			}
	
			
			if (GUILayout.Button("Asset-Store"))
			{
				Application.OpenURL(readme.assetStoreLink);
			}
			
			if (GUILayout.Button("Support"))
			{
				Application.OpenURL(readme.emailLink);
			}
			
		
			GUILayout.Label("<b>More by doorfortyfour</b>", m_BodyStyle);
			
			if (GUILayout.Button("TileWorldCreator"))
			{
				Application.OpenURL(readme.tileWorldCreatorLink);
			}
			
			if (GUILayout.Button("FlowReactor"))
			{
				Application.OpenURL(readme.flowreactorLink);
			}
	
	
			GUILayout.Label("<b>Changelog</b>", m_BodyStyle);
			Rect _lastRect = new Rect(0,0,0,0);
			if (Event.current.type == EventType.Repaint)
			{
				_lastRect = GUILayoutUtility.GetLastRect();
			}
			
			using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition, GUILayout.Width(_lastRect.width))) //, GUILayout.Height(100)))
			{
				scrollPosition = scrollView.scrollPosition;
				GUILayout.TextArea(readme.changelog, GUILayout.ExpandHeight(true));
			}
			
		}
		
		void LoadChangelog()
		{
			var _path = System.IO.Path.Combine(GetRelativePath(), "");
			_path = _path + "/Changelog.txt";
			
			//Read the text from directly from the test.txt file
			System.IO.StreamReader reader = new System.IO.StreamReader(_path);
			readme.changelog = reader.ReadToEnd();
			
			readme.version = System.IO.File.ReadLines(_path).First();
			
			reader.Close();
		}
	}
}