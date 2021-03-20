using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DataExternalDocument : DimController
{
    private string documentPath;

    /// <summary>
    /// Path to the external reference document
    /// </summary>
    public string DocumentPath
    {
        get { return documentPath; }

        set
        {
            documentPath = value;
            writeReferenceFile();
        }
    }

    /// <summary>
    /// Path to the external document metafile
    /// </summary>
    protected string metaFilePath { get; set; }

    /// <summary>
    /// External meta data instance
    /// </summary>
    protected ExternalMetaData internalMetaData { get; set; }

    /// <summary>
    /// Meta data in string format for storage
    /// </summary>
    protected string metaDataContent { get; set; }

    /// <summary>
    /// File loader for external document
    /// </summary>
    protected IExternalDocumentProvider documentLoader; 

    /// <summary>
    /// Exchange file extension to meta file extension
    /// </summary>
    protected void setPath()
    {
        metaFilePath = System.IO.Path.ChangeExtension(DocumentPath, ".itd");
    }

    /// <summary>
    /// Writing external document into meta data reference location
    /// </summary>
    public void writeReferenceFile()
    {
        setPath();
        internalMetaData.ReferenceFile = DocumentPath;
    }

    public DataExternalDocument(CoreApplication app) : base(app)
    {
        Debug.Log("MetaData created.");
    }

    public virtual void writeData()
    {
        if (System.IO.File.Exists(metaFilePath))
        {
            // if data is alredy exists
            // data is overwrite
            overWrite();
        }
        else
        {
            metaDataContent = internalMetaData.getMetaContent();
            WriteFile();
        }
    }

    public virtual void readData()
    {
        ReadFile();
    }

    protected virtual void overWrite()
    {
        OverwriteFile();
    }

    protected virtual void ReadFile()
    {
        if (System.IO.File.Exists(metaFilePath))
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(metaFilePath);
            metaDataContent = reader.ReadToEnd();
            reader.Close();
        }
    }

    protected void WriteFile()
    {
        Debug.Log(metaFilePath);
        System.IO.StreamWriter writer = new System.IO.StreamWriter(metaFilePath, true);
        writer.WriteLine(metaDataContent);
        writer.Close();
    }

    protected void OverwriteFile()
    {
        if (System.IO.File.Exists(metaFilePath))
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(metaFilePath, false))
            {
                writer.Write(string.Empty);
            }
            WriteFile();
            //Re-import the file to update the reference in the editor
            UnityEditor.AssetDatabase.ImportAsset(metaFilePath);
            TextAsset asset = Resources.Load(metaFilePath) as TextAsset;
        }
    }

    protected static Dictionary<string, string> parseData(string content)
    {
        Dictionary<string, string> keyValuePairs = content.Trim().Split('\n')
          .Select(value => value.Split(new string[] { " = " }, System.StringSplitOptions.RemoveEmptyEntries))
          .ToDictionary(pair => pair[0], pair => pair[1]);

        return keyValuePairs;
    }

    public string getReferenceFile()
    {
        return internalMetaData.ReferenceFile;
    }

    protected virtual void visualizeExternalDocument() { }

}
