using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ExternalDocumentLoaderFactory
{
    public static DataExternalDocument Create(CoreApplication app, string filePath)
    {
        string fileExtension = System.IO.Path.GetExtension(filePath).ToUpper();
        string temp = fileExtension.Replace(".", string.Empty).ToUpper();
        ExternalDocumentFileType type = (ExternalDocumentFileType)Enum.Parse(typeof(ExternalDocumentFileType), temp);
        Enum.GetNames(typeof(ExternalDocumentFileType));

        IExternalDocumentProvider FileLoader;

        switch (type)
        {
            case ExternalDocumentFileType.OBJ:
                ObjFileLoader _ObjFileLoader = new ObjFileLoader();
                return _ObjFileLoader.LoadDocument(app, filePath);
                break;

            case ExternalDocumentFileType.STL:
                StlFileLoader _StlFileLoader = new StlFileLoader();
                return _StlFileLoader.LoadDocument(app, filePath);
                break;
        }
        
        throw new NotImplementedException("No File loader for " + fileExtension + " implemented");
    }
}

public enum ExternalDocumentFileType
{
    STL,
    OBJ,
    PDF,
    JPG,
    PNG
}
