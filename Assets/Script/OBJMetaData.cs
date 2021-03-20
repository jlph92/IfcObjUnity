using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBJMetaData : ExternalMetaData
{
    public OBJMetaData(CoreApplication app, DimController controller) : base(app, controller)
    {
        DataType = ExternalDocumentFileType.OBJ;
    }

    public override string getMetaContent()
    {
        return System.String.Empty;
    }
}
