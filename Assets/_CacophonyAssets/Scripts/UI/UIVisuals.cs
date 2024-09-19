using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Author: Liz
/// Description: Base script for UI elements.
/// </summary>
public class UIVisuals
{
    /// <summary>
    /// Calculates the animation time of an animation.
    /// </summary>
    /// <param name="anims">The animator to search through.</param>
    /// <param name="animationName">The name of the animation to find.</param>
    /// <returns>The animation time if an animation is found. 0 otherwise, and flags a warning.</returns>
    public static float GetAnimationTime(Animator anims, string animationName)
    {
        AnimationClip[] clips = anims.runtimeAnimatorController.animationClips;
        foreach (AnimationClip c in clips)
        {
            if (c.name == animationName)
            {
                return c.length;
            }
        }

        Debug.LogWarning($"Could not find animation named {animationName} in {anims.name}");
        return 0;
    }
}
