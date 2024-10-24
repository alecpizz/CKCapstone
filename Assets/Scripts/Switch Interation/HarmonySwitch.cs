using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarmonySwitch : ParentSwitch
{
    //private Quaternion _originBeam = new Quaternion(0f, 180f, 0f, 0f);


    public override void SwitchActivation()
    {
        gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y + 180, 0f);
        print("I work");
    }

    public override void SwitchDeactivation()
    {
        gameObject.transform.eulerAngles = new Vector3(0f, gameObject.transform.eulerAngles.y - 180, 0f);

        print("I also work");
    }


}
