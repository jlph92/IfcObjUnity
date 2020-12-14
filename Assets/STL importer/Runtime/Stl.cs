using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Parabox.Stl
{
	/// <summary>
	/// Describes the file format of an STL file.
	/// </summary>
	public enum FileType
	{
		Ascii,
		Binary
	};

	public enum CoordinateSpace
	{
		Left,
		Right
	}

	public enum UpAxis
	{
		X,
		Y,
		Z
	}

	public enum Unit
	{
		Milimeter,
		Meter,
		Inch
	}

	/// <summary>
	/// Export STL files from Unity mesh assets.
	/// </summary>
	static class Stl
	{
		public static Vector3 ToCoordinateSpace(Vector3 point)
		{
			return new Vector3(point.x, -point.z, -point.y);
		}

		//public static Vector3 ToLeftAxis(Vector3 point, UpAxis modelUpAxis)
  //      {
  //          switch (modelUpAxis)
  //          {
		//		case UpAxis.X:
		//			return new Vector3(point.y, point.x, -point.z);
		//			break;
		//		case UpAxis.Y:
		//			return new Vector3(point.x, -point.y, -point.z);
		//			break;
		//		case UpAxis.Z:
		//			return new Vector3(point.x, -point.z, point.y);
		//			break;
		//		default:
		//			return new Vector3(point.x, point.y, point.z);
		//			break;
		//	}
  //      }

		//public static Vector3 ToRightAxis(Vector3 point, UpAxis modelUpAxis)
		//{
		//	switch (modelUpAxis)
		//	{
		//		case UpAxis.X:
		//			return new Vector3(-point.y, -point.x, -point.z);
		//			break;
		//		case UpAxis.Y:
		//			return new Vector3(point.z, -point.y, point.x);
		//			break;
		//		case UpAxis.Z:
		//			return new Vector3(point.x, point.z, -point.y);
		//			break;
		//		default:
		//			return new Vector3(point.x, point.y, point.z);
		//			break;
		//	}
		//}
	}
}
