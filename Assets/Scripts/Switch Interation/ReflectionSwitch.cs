using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionSwitch : ParentSwitch
{
    [SerializeField] ReflectiveObject mirror;

    public override void SwitchActivation()
    {
        //mirror.SetReflection();
    }

    public override void SwitchDeactivation()
    {
        //mirror.SetReflection();
    }
}
