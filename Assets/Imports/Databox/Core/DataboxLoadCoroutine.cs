using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

using Databox;
using Databox.FullSerializer;
#if NET_4_6
using Databox.OdinSerializer;
#endif
using Databox.Dictionary;
using Databox.Ed;

namespace Databox.Utils
{
	// used to load file from streaming assets folder on android platform and for importing google sheets at runtime
	public class DataboxLoadCoroutine : Singleton<DataboxLoadCoroutine>
	{
		
		/// <summary>
		/// Import google sheet runtime coroutine
		/// </summary>
		/// <param name="database"></param>
		/// <param name="_importType"></param>
		/// <returns></returns>
		public void ImportFromGoogle(DataboxObject _database, DataboxGoogleSheetDownloader.ImportType _importType)
		{
			StartCoroutine(ImportFromGoogleIE(_database, _importType));	
		}
		
		IEnumerator ImportFromGoogleIE(DataboxObject database, DataboxGoogleSheetDownloader.ImportType _importType)
		{
		
			DataboxCSVConverter.firstTimeReplace = true;
		
			for (int i = 0; i < database.googleWorksheets.Count; i ++)
			{
				var _url = database.googleSheetUrl;
			
				_url = FixURL( _url, database.googleWorksheets[i].id);

				UnityWebRequest _download = UnityWebRequest.Get(_url);
			
				yield return _download.SendWebRequest();
			
				while (_download.isDone == false)
					yield return null;
				
				// handle an unknown internet error
				#if UNITY_2020_1_OR_NEWER
				if (_download.result == UnityWebRequest.Result.ConnectionError)
				#else
				if (_download.isNetworkError || _download.isHttpError || !string.IsNullOrEmpty(_download.error))
				#endif
				{
					Debug.LogWarningFormat("Couldn't retrieve file at <{0}> and error message was: {1}", _download.url, _download.error);
				}
				else
				{ 
					// make sure the fetched file isn't just a Google login page
					if (_download.downloadHandler.text.Contains("google-site-verification")) {
						Debug.LogWarningFormat("Couldn't retrieve file at <{0}> because the Google Doc didn't have public link sharing enabled", _download.url);
						continue;
					}


					List<DataboxCSVConverter.Entry> entries = new List<DataboxCSVConverter.Entry>();
					DataboxCSVConverter.ConvertCSV(_download.downloadHandler.text, out entries);

				
					yield return new WaitForSeconds(1f);
				
				
					switch (_importType)
					{
					case DataboxGoogleSheetDownloader.ImportType.Append:
					#if DATABOX_LOCALIZATION
						DataboxCSVConverter.AppendToDB(database, database.googleWorksheets[i].name, entries, database.isLocalizationTable);
					#else
						DataboxCSVConverter.AppendToDB(database, database.googleWorksheets[i].name, entries);
					#endif
						break;
					case DataboxGoogleSheetDownloader.ImportType.Replace:
					#if DATABOX_LOCALIZATION
						DataboxCSVConverter.ReplaceDB(database, database.googleWorksheets[i].name, entries, database.isLocalizationTable);
					#else
						DataboxCSVConverter.ReplaceDB(database, database.googleWorksheets[i].name, entries);
					#endif
						break;
					}
				
				}
			
				_download.Dispose();
			}
		
		
			if (database.OnImportFromGoogleComplete != null)
			{
				database.OnImportFromGoogleComplete();
			}
		}
		
		public static string FixURL(string url, string gId)
		{
			// if it's a Google Docs URL, then grab the document ID and reformat the URL
			if (url.StartsWith("https://docs.google.com/document/d/"))
			{
				var docID = url.Substring( "https://docs.google.com/document/d/".Length, 44 );
				return string.Format("https://docs.google.com/document/export?gid={1}&format=txt&id={0}&includes_info_params=true", docID, gId);
			}
			if (url.StartsWith("https://docs.google.com/spreadsheets/d/"))
			{
				var docID = url.Substring( "https://docs.google.com/spreadsheets/d/".Length, 44 );
				return string.Format("https://docs.google.com/spreadsheets/export?gid={1}&format=csv&id={0}", docID, gId);
			}
            
			return url;
		}
		

		
		
		/// <summary>
		///  Android runtime load coroutine
		/// </summary>
		/// <param name="_savePath"></param>
		/// <param name="_databoxObject"></param>
		/// <param name="_callEvent"></param>
		
		public void LoadCoroutine(string _savePath, DataboxObject _databoxObject, bool _callEvent)
		{
			StartCoroutine(LoadCoroutineIE(_savePath, _databoxObject, _callEvent));
		}
	
		public IEnumerator LoadCoroutineIE(string _savePath, DataboxObject _databoxObject, bool _callEvent)
		{
		
			string _j = "";
	
			using (UnityEngine.Networking.UnityWebRequest _download = UnityEngine.Networking.UnityWebRequest.Get(_savePath))
			{
				yield return _download.SendWebRequest();
					
				while (!_download.isDone)
				{
					#if UNITY_2020_1_OR_NEWER
					if (_download.result == UnityWebRequest.Result.ConnectionError)
					#else
					if (_download.isNetworkError || _download.isHttpError || !string.IsNullOrEmpty(_download.error))
					#endif
					{
						break;
					}
					
					yield return null;
				}
							
				#if UNITY_2020_1_OR_NEWER
				if (_download.result == UnityWebRequest.Result.ConnectionError)
				#else
				if (_download.isNetworkError || _download.isHttpError || !string.IsNullOrEmpty(_download.error))
				#endif
				{
					Debug.Log(_download.error);
				}
				else
				{
					_j = _download.downloadHandler.text;
				}
			}
			
			
		
				
			if (!string.IsNullOrEmpty(_j))
			{	
				if (_databoxObject.serializer == DataboxObject.Serializer.FullSerializer)
				{
					DeserializeWithFullSerializer(_j, _databoxObject, _callEvent);	
				}
				else
				{
#if NET_4_6
					DeserializeWithOdin(_j, _databoxObject, _callEvent);
#endif
				}
			}
			else
			{
				// destroy this object
				//Destroy(this.gameObject);
			}
		}
	
	
		void DeserializeWithFullSerializer(string _text, DataboxObject _databoxObject, bool _callEvent)
		{
			if (_databoxObject.encryption)
			{
				_text = _databoxObject.XOREncryptDecrypt(_text, _databoxObject.encryptionKey);
			}
			
			fsSerializer _serializer = new fsSerializer();
			fsData data = fsJsonParser.Parse(_text);
					
			// step 2: deserialize the data
			object deserialized = null;
			_serializer.TryDeserialize(data, typeof(OrderedDictionary<string, DataboxObject.Database>), ref deserialized);
					
			var _a = (OrderedDictionary<string, DataboxObject.Database>)deserialized as OrderedDictionary<string, DataboxObject.Database>;
			_databoxObject.DB = _a;
					
			if (_callEvent && _databoxObject.OnDatabaseLoaded != null && Application.isPlaying)
			{
				_databoxObject.OnDatabaseLoaded();
			}
					
			_databoxObject.databaseLoaded = true;
				
			if (_databoxObject.debugMode)
			{
				Debug.Log("Databox: Database loaded");
			}
		
			// destroy this object
			//Destroy(this.gameObject);
		}
	
#if NET_4_6
		void DeserializeWithOdin(string _text, DataboxObject _databoxObject, bool _callEvent)
		{
			
			var _jsonLegacy = false;
			
			DataFormat _format = DataFormat.Binary;
			switch (_databoxObject.dataFormat)
			{
			case DataboxObject.OdinDataFormat.binary:
				_format = DataFormat.Binary;
				break;
			case DataboxObject.OdinDataFormat.json:
				_format = DataFormat.JSON;
				break;
			case DataboxObject.OdinDataFormat.json_legacy:
				_jsonLegacy = true;
				_format = DataFormat.JSON;
				break;
			}
			
			
			byte[] bytes;
			
			if (_format == DataFormat.JSON && !_jsonLegacy)
			{
				bytes = System.Text.Encoding.UTF8.GetBytes(_text);
			}
			else
			{
				bytes = Convert.FromBase64String(_text);
			}
			
			
			if (_databoxObject.encryption)
			{
				bytes = _databoxObject.ByteXOREncryptDecrypt(bytes, _databoxObject.encryptionKey);
			}
			
			Stream _stream = new MemoryStream(bytes);
	
			using (var reader = new BinaryReader(_stream))
			{
				_databoxObject.DB = SerializationUtility.DeserializeValue<OrderedDictionary<string, DataboxObject.Database>>( reader.ReadBytes((int)_stream.Length), _format);
			}
			
			if (_callEvent && _databoxObject.OnDatabaseLoaded != null && Application.isPlaying)
			{
				_databoxObject.OnDatabaseLoaded();
			}
					
			_databoxObject.databaseLoaded = true;
				
			if (_databoxObject.debugMode)
			{
				Debug.Log("Databox: Database loaded");
			}
		
			// destroy this object
			//Destroy(this.gameObject);
		}
#endif
	}
}
