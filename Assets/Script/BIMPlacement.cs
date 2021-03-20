using System.Collections.Generic;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;
using UnityEngine;

public class BIMPlacement
{
    public Matrix4x4 parentMatrix { get; set; }

    /// <summary>
    /// This function centralises the extraction of a product placement, but it needs the support of XbimPlacementTree and an XbimGeometryEngine
    /// We should probably find a conceptual place for it somewhere in the scene, where these are cached.
    /// </summary>
    public static XbimMatrix3D GetTransform(IIfcProduct product, BIMPlacement tree)
    {
        XbimMatrix3D placementTransform = XbimMatrix3D.Identity;
        if (product.ObjectPlacement is IIfcLocalPlacement)
            placementTransform = tree[product.ObjectPlacement.EntityLabel];
        //else if (product.ObjectPlacement is IIfcGridPlacement)
        //    placementTransform = Xbim.Ifc4.GeometricConstraintResource.IfcObjectPlacement.ToMatrix3D(product as IIfcGridPlacement);
        return placementTransform;
    }

    /// <summary>
    ///     Builds a placement tree of all ifcLocalPlacements
    /// </summary>
    /// <param name="model"></param>
    /// <param name="adjustWcs">
    ///     If there is a single root displacement, this is removed from the tree and added to the World
    ///     Coordinate System. Useful for models where the site has been located into a geographical context
    /// </param>
    public BIMPlacement(IModel model, bool adjustWcs = true)
    {
        var rootNodes = new List<XbimPlacementNode>();
        var localPlacements = model.Instances.OfType<IIfcLocalPlacement>(true).ToList();
        Nodes = new Dictionary<int, XbimPlacementNode>();
        foreach (var placement in localPlacements)
        {
            //Debug.Log("Added: " + placement.EntityLabel);
            Nodes.Add(placement.EntityLabel, new XbimPlacementNode(placement));
        }
        foreach (var localPlacement in localPlacements)
        {
            if (localPlacement.PlacementRelTo != null) //resolve parent
            {
                var xbimPlacement = Nodes[localPlacement.EntityLabel];
                var xbimPlacementParent = Nodes[localPlacement.PlacementRelTo.EntityLabel];
                xbimPlacement.Parent = xbimPlacementParent;
                xbimPlacementParent.Children.Add(xbimPlacement);
            }
            else
                rootNodes.Add(Nodes[localPlacement.EntityLabel]);
        }
        if (adjustWcs && rootNodes.Count == 1)
        {
            var root = rootNodes[0];
            WorldCoordinateSystem = root.Matrix;
            //make the children parentless
            foreach (var node in Nodes.Values.Where(node => node.Parent == root)) node.Parent = null;
            root.Matrix = new XbimMatrix3D(); //set the matrix to identity
        }
        //muliply out the matrices
        foreach (var node in Nodes.Values) node.ToGlobalMatrix();
    }

    public XbimMatrix3D WorldCoordinateSystem { get; private set; }

    private Dictionary<int, XbimPlacementNode> Nodes { get; set; }

    public XbimMatrix3D this[int placementLabel]
    {
        get
        {
            return Nodes[placementLabel].Matrix;
        }
    }

    public class XbimPlacementNode
    {
        private List<XbimPlacementNode> _children;
        private bool _isAdjustedToGlobal;

        public XbimPlacementNode(IIfcLocalPlacement placement)
        {
            PlacementLabel = placement.EntityLabel;
            Matrix = placement.RelativePlacement.ToMatrix3D();
            _isAdjustedToGlobal = false;
        }

        public int PlacementLabel { get; private set; }
        public XbimMatrix3D Matrix { get; protected internal set; }

        public List<XbimPlacementNode> Children
        {
            get { return _children ?? (_children = new List<XbimPlacementNode>()); }
        }

        public XbimPlacementNode Parent { get; set; }

        internal void ToGlobalMatrix()
        {
            if (!_isAdjustedToGlobal && Parent != null)
            {
                Parent.ToGlobalMatrix();
                Matrix = Matrix * Parent.Matrix;
            }
            _isAdjustedToGlobal = true;
        }
    }

    public static Matrix4x4 translateUnityMatrix(XbimMatrix3D xBIM_Matrix)
    {
        Matrix4x4 matrix = Matrix4x4.identity;

        //Debug.Log(xBIM_Matrix.ToString());

        Vector4 Row_1 = new Vector4((float) xBIM_Matrix.M11, (float)xBIM_Matrix.M12, (float)xBIM_Matrix.M13, (float)xBIM_Matrix.M14);
        Vector4 Row_2 = new Vector4((float)xBIM_Matrix.M21, (float)xBIM_Matrix.M22, (float)xBIM_Matrix.M23, (float)xBIM_Matrix.M24);
        Vector4 Row_3 = new Vector4((float)xBIM_Matrix.M31, (float)xBIM_Matrix.M32, (float)xBIM_Matrix.M33, (float)xBIM_Matrix.M34);
        Vector4 Row_4 = new Vector4((float)xBIM_Matrix.OffsetX, (float)xBIM_Matrix.OffsetY, (float)xBIM_Matrix.OffsetZ, (float)xBIM_Matrix.M44);

        matrix.SetColumn(0, Row_1);
        matrix.SetColumn(1, Row_2);
        matrix.SetColumn(2, Row_3);
        matrix.SetColumn(3, Row_4);

        return matrix;
    }

    public Vector3 getProductOrigin(IIfcProduct AttachedProduct)
    {
        XbimMatrix3D placementTransform = BIMPlacement.GetTransform(AttachedProduct, this);
        parentMatrix = BIMPlacement.translateUnityMatrix(placementTransform);
        Vector3 point = parentMatrix.MultiplyPoint3x4(Vector3.zero);

        Debug.LogFormat("BIM Coordinate Read : {0}", point);
        
        Vector3 result = new Vector3(point.x, point.z, point.y);

        return result;
    }

}