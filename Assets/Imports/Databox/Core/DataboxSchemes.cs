using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Databox.Dictionary;

namespace Databox
{
	[System.Serializable]
	[CreateAssetMenu(menuName = "Databox/New Databox Data Schemes")]
	public class DataboxSchemes : ScriptableObject
	{
		
		[System.Serializable]
		public class Schemes
		{
			public string schemeName;
			public bool foldout;
			
			[System.Serializable]
			public class SchemeType
			{
				public string name;
				public string type;
				public int typeIndex;
			} 
			  
			public Schemes (string _name)
			{
				schemeName = _name;
			}
			
			[SerializeField]
			public List<SchemeType> schemeTypes = new List<SchemeType>();
		}
		
		[SerializeField]
		public List<Schemes> schemes = new List<Schemes>();
	
	}
}