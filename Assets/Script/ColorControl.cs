using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorControl
{
    public static Color getColor(DamageTypes damageType)
    {
        switch (damageType)
        {
            case DamageTypes.Crack :
                return Color.red;
                break;

            case DamageTypes.Spalling:
                return Color.cyan;
                break;
            case DamageTypes.Rusting:
                return Color.green;
                break;

            case DamageTypes.Decolorisation:
                return Color.yellow;
                break;
            case DamageTypes.Vegetation:
                return Color.magenta;
                break;
        }

        return Color.white;
    }
}
