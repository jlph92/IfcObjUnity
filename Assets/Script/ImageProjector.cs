using UnityEngine;

public class ImageProjector : MonoBehaviour
{
    Projector projector;
    public float depth = 0.0f;
    GameObject assignObject;

    void Start()
    {
        projector = GetComponent<Projector>();
        // Use the Projector shader on the material
        projector.material.shader = Shader.Find("Projector/Texture");
        setShaderDir();
    }

    void Update()
    {
        if (transform.hasChanged) setShaderDir();
    }

    void setShaderDir()
    {
        Vector3 dir = transform.forward.normalized;
        Vector4 projectorDir = new Vector4(dir.x, dir.y, dir.z);
        projector.material.SetVector("_ProjectorDir", projectorDir);
    }
}
