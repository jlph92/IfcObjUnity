using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OBJDocumentData : DataExternalDocument
{
    public OBJDocumentData(CoreApplication app) : base(app)
    {
        internalMetaData = new OBJMetaData(app, this);
        writeReferenceFile();
    }

    public override void readData()
    {
        base.readData();
        Dictionary<string, string> keyValuePairs = parseData(metaDataContent);
    }

    public override void writeData()
    {
        base.writeData();
    }

    protected override void overWrite()
    {
        base.overWrite();
    }

    protected override void visualizeExternalDocument()
    {
        //new OBJDataVisualization(app).GenerateGameObject(this);
    }
}
