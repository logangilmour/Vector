using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTest : MonoBehaviour
{
    Material mat;
    public Vector2 B0;
    public Vector2 B1;
    public Vector2 B2;
    // Start is called before the first frame update
    void Start()
    {
        mat = new Material(Shader.Find("Hidden/Bez"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        Shader.SetGlobalVector("_B0", B0);
        Shader.SetGlobalVector("_B1", B1);
        Shader.SetGlobalVector("_B2", B2);

        Graphics.Blit(source, destination,mat);
    }
}
