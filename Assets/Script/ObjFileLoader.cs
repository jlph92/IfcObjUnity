using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ObjFileLoader : IExternalDocumentProvider
{
    public DataExternalDocument LoadDocument(CoreApplication app, string pathToFile)
    {
        try
        {
            OBJDocumentData _OBJDocumentData = new OBJDocumentData(app);
            _OBJDocumentData.DocumentPath = pathToFile;

            return _OBJDocumentData;
        }
        catch (NotImplementedException notImp)
        {
            throw new NotImplementedException();
        }

        return null;
    }
}
