using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "Databox/New Readme")]
public class DataboxReadme : ScriptableObject
{
	public string version = "";
	public string text = "Thank you for purchasing Databox and supporting a small indie game studio!" +
		"\n\n" +
		"<b>Demo scenes</b> \n" +
		"Simply unpack the unity package located in the folder: \n<i>Databox / Demo </i> \n\n";
	
	public string documentationLink = "http://databox.doorfortyfour.com/documentation";
	public string websiteLink = "http://databox.doorfortyfour.com";
	public string assetStoreLink = "https://assetstore.unity.com/packages/tools/utilities/databox-data-editor-save-solution-155189";
	public string emailLink = "mailto:mail@doorfortyfour.com";
	
	public string flowreactorLink = "https://assetstore.unity.com/packages/tools/visual-scripting/flowreactor-high-level-visual-scripting-167519";
	public string tileWorldCreatorLink = "https://u3d.as/2Dz4";
	
	public string changelog = "";
}