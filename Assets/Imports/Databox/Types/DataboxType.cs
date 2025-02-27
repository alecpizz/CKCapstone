using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
#if NET_4_6
using Databox.OdinSerializer;
#endif

namespace Databox
{
	/// <summary>
	/// Databox Type
	/// Inherit from DataboxType to make sure your class will be supported by Databox.
	/// Several virtual methods are available to integrate your class into Databox.
	/// </summary>
	[System.Serializable]
	public class DataboxType
	{
		/// <summary>
		/// used to reset variables back to its initial value
		/// </summary>
		public virtual void Reset(){}
		/// <summary>
		/// draw custom editor gui fields
		/// </summary>
		public virtual void DrawEditor(){} 
		/// <summary>
		/// same as DrawEditor() but with additional DataboxObject parameter.
		/// </summary>
		/// <param name="_databoxObject"></param>
		public virtual void DrawEditor(DataboxObject _databoxObject) {}
		/// <summary>
		/// draw custom editor gui only for init values
		/// </summary>
		public virtual void DrawInitValueEditor(){}
		/// <summary>
		/// Used for the cloud sync comparison. Compare the input value with the current value and return a string if there's a change.
		/// </summary>
		/// <param name="_value"></param>
		/// <returns></returns>
		public virtual string Equal(DataboxType _value){return "";}
		/// <summary>
		/// Convert csv string value to DataboxType value
		/// </summary>
		/// <param name="_value"></param>
		public virtual void Convert(string _value){}
		/// <summary>
		/// Used for localization data, instead of passing a single value from a csv file. We pass the whole row for each language
		/// </summary>
		/// <param name="_rowValue"></param>
		public virtual void ConvertRow(DataboxCSVConverter.Entry _rowValue){}
		
		public delegate void ValueChanged(DataboxType _data);
		public ValueChanged OnValueChanged;
		
		public DataboxType (){}
	}
	
	
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
	public class DataboxTypeAttribute : Attribute
	{
		public string Name{get;set;}
	}
}