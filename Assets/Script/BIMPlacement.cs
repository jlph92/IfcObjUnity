using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using UnityEngine;

public class BIMPlacement
{
    /// <summary>
    ///     Builds a placement tree of all ifcLocalPlacements
    /// </summary>
    /// <param name="model"></param>
    /// <param name="adjustWcs">
    ///     If there is a single root displacement, this is removed from the tree and added to the World
    ///     Coordinate System. Useful for models where the site has been located into a geographical context
    /// </param>
    public static void extractLocation(IfcModel ifcModel, IfcStore model)
    {
        Debug.Log("**************** Extract local placement. ***********************");

        // Extract Local Placement Node
        var localPlacements = model.Instances.OfType<IIfcLocalPlacement>(true);
        var Nodes = new Dictionary<int, XbimPlacementNode>();

        foreach (var localPlacement in localPlacements)
        {
            Nodes.Add(localPlacement.EntityLabel, new XbimPlacementNode(localPlacement));
        }

        foreach (var localPlacement in localPlacements)
        {
            if (localPlacement.PlacementRelTo != null) //resolve parent
            {
                var xbimPlacement = Nodes[localPlacement.EntityLabel];
                var xbimPlacementParent = Nodes[localPlacement.PlacementRelTo.EntityLabel];
                xbimPlacement.Parent = xbimPlacementParent;
            }
        }

        foreach (XbimPlacementNode node in Nodes.Values)
        {
            node.computePosition(ifcModel);
        }
    }
}

/// <summary>
/// This function centralises the extraction of a product placement, but it needs the support of XbimPlacementTree and an XbimGeometryEngine
/// We should probably find a conceptual place for it somewhere in the scene, where these are cached.
/// </summary>
public class XbimPlacementNode
{
    public XbimPlacementNode Parent { get; set; }
    XbimMatrix3D LocalMatrix { get; set; }
    XbimMatrix3D GlobalMatrix { get; set; }
    public Matrix4x4 GlobalUnityMatrix { get; private set; }
    private bool isGlobal { get; set; }

    private IIfcLocalPlacement placement;

    // Construct XbimPlacementNode to IfcModel
    public XbimPlacementNode(IIfcLocalPlacement placement)
    {
        this.placement = placement;
        isGlobal = false;
    }

    // Return World Coordinate / Or so called Absolute Coordinate
    internal void ToGlobalMatrix()
    {
        if (Parent != null)
        {
            if (!isGlobal)
            {
                if (!Parent.isGlobal) Parent.ToGlobalMatrix();
                GlobalMatrix = LocalMatrix * Parent.GlobalMatrix;
                isGlobal = true;
            }
        }

        else return;
    }

    public void computePosition(IfcModel rootModel)
    {
        var localPosition = getUnityLocalPosition();
        var globalPosition = getUnityGlobalPosition();

        if (placement == null) return;

        foreach (var ifcProduct in placement.PlacesObject)
        {
            // Debug.LogFormat("Placement Name: {0}, Entity Label: {1}", ifcProduct.Name, ifcProduct.EntityLabel);

            var ifcModel = IfcModel.getIfcModel(rootModel, ifcProduct.EntityLabel);

            //Debug.LogFormat("Placement Name: {0}, Entity Label: {1}", ifcModel.Name, ifcModel.EntityLabel);

            if (ifcModel != null)
            {
                ifcModel.placementNode = this;
                LocalMatrix = placement.RelativePlacement.ToMatrix3D();

                // Insert Location for IfcModel
                ifcModel.localPosition = localPosition;
                ifcModel.position = globalPosition;

                Debug.LogFormat("{0} in BIM Local Coordinate Read : {1}", ifcModel.Name, LocalMatrix);
                Debug.LogFormat("{0} in BIM World Coordinate Read : {1}", ifcModel.Name, GlobalMatrix);
                Debug.LogFormat("{0} in Unity World Coordinate Read : {1}", ifcModel.Name, GlobalUnityMatrix);
            }
        }
    }

    // Extract global placement from Entity
    Vector3 getUnityGlobalPosition()
    {
        if (GlobalMatrix.IsIdentity) ToGlobalMatrix();

        // Convert to Unity Matrix
        GlobalUnityMatrix = translateUnityMatrix(this.GlobalMatrix);
        // Move point under Matrix
        Vector3 point = GlobalUnityMatrix.MultiplyPoint3x4(Vector3.zero);

        //Debug.LogFormat("BIM World Coordinate Read : {0}", point);

        Vector3 result = new Vector3(point.x, point.z, point.y);

        return result;
    }

    // Extract local placement from Entity
    Vector3 getUnityLocalPosition()
    {
        // Convert to Unity Matrix
        var localMatrix = translateUnityMatrix(this.LocalMatrix);
        // Move point under Matrix
        Vector3 point = localMatrix.MultiplyPoint3x4(Vector3.zero);

        //Debug.LogFormat("BIM World Coordinate Read : {0}", point);

        Vector3 result = new Vector3(point.x, point.z, point.y);

        return result;
    }

    Matrix4x4 translateUnityMatrix(XbimMatrix3D xBIM_Matrix)
    {
        Matrix4x4 matrix = Matrix4x4.identity;

        Vector4 Row_1 = new Vector4((float)xBIM_Matrix.M11, (float)xBIM_Matrix.M12, (float)xBIM_Matrix.M13, (float)xBIM_Matrix.M14);
        Vector4 Row_2 = new Vector4((float)xBIM_Matrix.M21, (float)xBIM_Matrix.M22, (float)xBIM_Matrix.M23, (float)xBIM_Matrix.M24);
        Vector4 Row_3 = new Vector4((float)xBIM_Matrix.M31, (float)xBIM_Matrix.M32, (float)xBIM_Matrix.M33, (float)xBIM_Matrix.M34);
        Vector4 Row_4 = new Vector4((float)xBIM_Matrix.OffsetX, (float)xBIM_Matrix.OffsetY, (float)xBIM_Matrix.OffsetZ, (float)xBIM_Matrix.M44);

        matrix.SetColumn(0, Row_1);
        matrix.SetColumn(1, Row_2);
        matrix.SetColumn(2, Row_3);
        matrix.SetColumn(3, Row_4);

        return matrix;
    }
}