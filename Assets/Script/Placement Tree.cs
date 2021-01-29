using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

public class PlacementTree
{
    // Update is called once per frame
    public static void BuildTree(IModel model)
    {
        string data = "";
        foreach (var product in model.Instances.OfType<IIfcProduct>())
        {
            var indent = "";
            data += ($"Product #{product.EntityLabel}={product.GetType().Name.ToUpperInvariant()}");
            var placement = product.ObjectPlacement;
            while (placement != null)
            {
                indent += "+";
                if (placement is IIfcGridPlacement gridPlacement)
                {
                    // handle grid placement
                    data += ($"\n{indent}Grid placement");
                }
                else if (placement is IIfcLocalPlacement localPlacement)
                {
                    // handle local placement
                    if (localPlacement.RelativePlacement is IIfcAxis2Placement3D ap3d)
                    {
                        data += ($"\n{indent}Placement 3D:");
                        data += ($"\n{indent}Location: {ap3d.Location.ToString()}");
                        //Debug.Log($"{indent}Orientation X: {ap3d.RefDirection.ToString()}");
                        //Debug.Log($"{indent}Orientation Z: {ap3d.Axis.ToString()}");
                    }
                    else if (localPlacement.RelativePlacement is IIfcAxis2Placement2D ap2d)
                    {
                        data += ($"\n{indent}Placement 2D:");
                        data += ($"\n{indent}Location: {ap2d.Location.ToString()}");
                        data += ($"\n{indent}Orientation X: {ap2d.RefDirection.ToString()}");
                    }

                    // walk up the placement tree
                    placement = localPlacement.PlacementRelTo;
                    continue;
                }
                break;
            }
        }
        Debug.Log(data);
    }
}
