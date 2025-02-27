using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using System.Text.RegularExpressions;

using Databox.CSVReader;

public class DataboxCSVReader : MonoBehaviour 
{
	public TextAsset csvFile; 
	public void Start()
	{
		string[,] grid = SplitCsvGrid(csvFile.text);
		Debug.Log("size = " + (1+ grid.GetUpperBound(0)) + "," + (1 + grid.GetUpperBound(1))); 
 
		DebugOutputGrid(grid); 
	}
 
	// outputs the content of a 2D array, useful for checking the importer
	static public void DebugOutputGrid(string[,] grid)
	{
		string textOutput = ""; 
		for (int y = 0; y < grid.GetUpperBound(1); y++) {	
			for (int x = 0; x < grid.GetUpperBound(0); x++) {
 
				textOutput += grid[x,y]; 
				textOutput += "|"; 
			}
			textOutput += "\n"; 
		}
		Debug.Log(textOutput);
	}
	
	/// <summary>
	/// Split a CSV string into a usable dictionary for Databox using the CSVReader class
	/// </summary>
	/// <param name="_input"></param>
	/// <returns></returns>
	public static Dictionary<int, List<string>> SplitCSV(string _input)
	{
		Dictionary<int, List<string>> data = new Dictionary<int, List<string>>();
		int _rowCount = 0;
		
		byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(_input);

		System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);
		System.IO.StreamReader reader = new System.IO.StreamReader(stream);
	
	
		using (CSVReader cr = new CSVReader(reader)) 
		{
			data.Add(_rowCount, new List<string>());
			
		
			for(int h = 0; h < cr.Headers.Length; h ++)
			{
				data[_rowCount].Add(cr.Headers[h]);
			}
				
			_rowCount++;
			data.Add(_rowCount, new List<string>());
		
			foreach (string[] line in cr)
			{
				for (int i = 0; i < line.Length; i ++)
				{
					data[_rowCount].Add(line[i]);
				}
				
				_rowCount++;
				data.Add(_rowCount, new List<string>());
			}
			
		}
		
		var returnData = Transpose(data);
		
		return returnData;
	}
 
 
	static Dictionary<int, List<string>> Transpose(Dictionary<int, List<string>> dt)
	{
		Dictionary<int, List<string>> dtNew = new Dictionary<int, List<string>>();

		
		// get max columns
		var _columnCount = 0;
		foreach(var row in dt.Keys)
		{	
			if (dt[row].Count > _columnCount)
			{
				_columnCount = dt[row].Count;
			}
		}
		
		
		for (int c = 0; c < _columnCount; c ++)
		{		
			dtNew.Add(c, new List<string>());
			
			foreach(var row in dt.Keys)
			{
				if (c < dt[row].Count)
				{
					dtNew[c].Add(dt[row][c]);
				}
			}
		}
		
		return dtNew;

	}
 
 
	///////////////////
	// OLD CSV Parser
	///////////////////
 
	// splits a CSV file into a 2D string array
	static public string[,] SplitCsvGrid(string csvText)
	{
		string[] lines = csvText.Split("\n"[0]); 
	
	
		// finds the max width of row
		int width = 0; 
		for (int i = 0; i < lines.Length; i++)
		{
			string[] row = SplitCsvLine( lines[i] ); 
			width = Mathf.Max(width, row.Length); 
		}
 
		// creates new 2D string grid to output to
		string[,] outputGrid = new string[width + 1, lines.Length + 1]; 
		for (int y = 0; y < lines.Length; y++)
		{
			string[] row = SplitCsvLine( lines[y] ); 
			for (int x = 0; x < row.Length; x++) 
			{
				outputGrid[x,y] = row[x]; 
 
				// This line was to replace "" with " in the output. 
				outputGrid[x,y] = outputGrid[x,y].Replace("\"\"", "\"");
				//Debug.Log("OUTPUT: " + outputGrid[x, y]);
			}
		}
 
		return outputGrid; 
	}
 
	// splits a CSV row 
	static public string[] SplitCsvLine(string line)
	{
		return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
			@"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", 
			System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
			select m.Groups[1].Value).ToArray();
	}
}