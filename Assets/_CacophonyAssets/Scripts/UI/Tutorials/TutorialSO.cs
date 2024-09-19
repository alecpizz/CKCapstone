using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Author: Liz
/// Description: Holds Tutorial data.
/// </summary>
[CreateAssetMenu()]
public class TutorialSO : ScriptableObject
{
    public string TutorialName;
    [TextArea(3, 10)] public string TutorialDescription;
    public Sprite TutorialImage;
    public VideoClip TutorialVideo;
}
