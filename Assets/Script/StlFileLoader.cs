using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class StlFileLoader : IExternalDocumentProvider
{
    public DataExternalDocument LoadDocument(CoreApplication app, string pathToFile)
    {
        try
        {
            STLDocumentData _STLDocumentData = new STLDocumentData(app);
            _STLDocumentData.DocumentPath = pathToFile;

            return _STLDocumentData;
        }
        catch (NotImplementedException notImp)
        {
            throw new NotImplementedException();
        }

        return null;
    }
}
