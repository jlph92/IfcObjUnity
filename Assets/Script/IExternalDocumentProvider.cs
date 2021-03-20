using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExternalDocumentProvider
{
    DataExternalDocument LoadDocument(CoreApplication app, string pathToFile);
}
