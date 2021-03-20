using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExternalDataVisualization
{
    GameObject GenerateGameObject(DataExternalDocument externalDocumentData);
}