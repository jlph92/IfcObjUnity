using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Geometry;
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
    public static void extractLocation(IfcModel ifcModel)
    {
        // Extract Local Placement Node
        var localPlacements = ifcModel.Entity.Model.Instances.OfType<IIfcLocalPlacement>(true)
            .Select(node => new XbimPlacementNode(node, ifcModel));
    }
}

/// <summary>
/// This function centralises the extraction of a product placement, but it needs the support of XbimPlacementTree and an XbimGeometryEngine
/// We should probably find a conceptual place for it somewhere in the scene, where these are cached.
/// </summary>
public class XbimPlacementNode
{
    public IfcModel ifcModel { get; private set; }
    XbimMatrix3D LocalMatrix { get; set; }
    XbimMatrix3D GlobalMatrix { get; set; }

    // Construct XbimPlacementNode to IfcModel
    public XbimPlacementNode(IIfcLocalPlacement placement, IfcModel rootModel)
    {
        ifcModel = IfcModel.getIfcModel(rootModel, placement.EntityLabel);
        ifcModel.placementNode = this;
        LocalMatrix = placement.RelativePlacement.ToMatrix3D();

        // Insert Location for IfcModel
        ifcModel.localPosition = getUnityLocalPosition();
        ifcModel.position = getUnityGlobalPosition();
    }

    // Return World Coordinate / Or so called Absolute Coordinate
    internal void ToGlobalMatrix()
    {
        var Parent = ifcModel.Parent.placementNode;
        if (!GlobalMatrix.IsIdentity && Parent != null)
        {
            if (!Parent.GlobalMatrix.IsIdentity) Parent.ToGlobalMatrix();
            GlobalMatrix = LocalMatrix * Parent.GlobalMatrix;
        }
        else return;
    }

    // Extract global placement from Entity
    Vector3 getUnityGlobalPosition()
    {
        if (GlobalMatrix.IsIdentity) ToGlobalMatrix();

        if (!GlobalMatrix.IsIdentity)
        {
            // Convert to Unity Matrix
            var globalMatrix = translateUnityMatrix(this.GlobalMatrix);
            // Move point under Matrix
            Vector3 point = globalMatrix.MultiplyPoint3x4(Vector3.zero);

            //Debug.LogFormat("BIM World Coordinate Read : {0}", point);

            Vector3 result = new Vector3(point.x, point.z, point.y);

            return result;
        }

        else return Vector3.zero;
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