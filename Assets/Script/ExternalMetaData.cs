using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExternalMetaData
{
    ExternalDocumentFileType DataType { get; set; }

    string ReferenceFile { get; set; }

    string getMetaContent();
}

public class ExternalMetaData : DimModel, IExternalMetaData
{
    public ExternalMetaData(CoreApplication app, DimController controller) : base(app, controller)
    {

    }

    public ExternalDocumentFileType DataType { get; set; }

    public string ReferenceFile { get; set; }

    public virtual string getMetaContent() { return System.String.Empty; }
}
