using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cacophony
{
    public interface IHarmonizable
    {
        void SetHarmonized(bool harmonized, HarmonizationType hType);

        bool IsHarmonized();

        bool IsPermaHarmonizable();

        HarmonizationObjectCatagory GetHarmonizationCatagory();

        HarmonizationType GetHarmonizationType();
    }

    public enum HarmonizationObjectCatagory
    {
        Enemy,
        Environment
    }
}