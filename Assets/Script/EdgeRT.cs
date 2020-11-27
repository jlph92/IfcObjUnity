using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class EdgeRT : MonoBehaviour {

    [HideInInspector]
    [SerializeField]
    private Camera _camera;
    private int currentWidth;
    private int currentHeight;

    private string _globalTextureName = "_GlobalEdgeTex";

    void SetupRT()
    {
        // let's only render depth and normals...
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.DepthNormals;

        if (_camera.targetTexture != null)
        {
            RenderTexture temp = _camera.targetTexture;
            _camera.targetTexture = null;
            DestroyImmediate(temp);
        }

        // ... to a RenderTexture 
        _camera.targetTexture = new RenderTexture(_camera.pixelWidth, _camera.pixelHeight, 16);
        _camera.targetTexture.filterMode = FilterMode.Bilinear;

        // we don't actually need this:
        //Shader.SetGlobalTexture(_globalTextureName, _camera.targetTexture);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHeight != Screen.currentResolution.height || currentWidth != Screen.currentResolution.width)
        {
            currentHeight = Screen.height;
            currentWidth = Screen.width;
            SetupRT();
        }

    }
}
