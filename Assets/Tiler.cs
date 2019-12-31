using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiler : MonoBehaviour
{
    public ComputeShader shader;
    ComputeBuffer[] tiles = new ComputeBuffer[2];
    ComputeBuffer[] counts = new ComputeBuffer[2];
    ComputeBuffer buffer;
    ComputeBuffer index;
    Vector4[] data;
    int[] tileSize = {128,16 };
    int numLines = 10000;
    int[] tileLines = { 10000, 1000 };
    int[] kernels = new int[2];
    

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<2; i++)
        {
            tiles[i] = new ComputeBuffer((Screen.width / tileSize[i]) * (Screen.height / tileSize[i]) * tileLines[i], 4);
            counts[i] = new ComputeBuffer((Screen.width / tileSize[i]) * (Screen.height / tileSize[i]), 4);
        }

        mat = new Material(Shader.Find("Hidden/Bez"));
    }

    private void OnPreRender()
    {
        

        if (buffer == null)
        {
            data = new Vector4[numLines];
            for (int i = 0; i < numLines; i++)
            {
                float dist = i / (float)numLines;
                float theta = i / (float)numLines * 10*2*Mathf.PI;
                Vector2 p1 = new Vector2(Mathf.Cos(theta) * dist, Mathf.Sin(theta) * dist);

                dist = (i+1) / (float)numLines;
                theta = (i+1) / (float)numLines * 10 * 2 * Mathf.PI;
                Vector2 p2 = new Vector2(Mathf.Cos(theta) * dist, Mathf.Sin(theta) * dist);

                data[i] = new Vector4(p1.x, p1.y, p2.x, p2.y);
            }
            buffer = new ComputeBuffer(data.Length, 4 * 4);
            buffer.SetData(data);

            int[] indexList = new int[numLines];
            for (int i = 0; i < numLines; i++)
            {
                indexList[i] = i;
            }
            index = new ComputeBuffer(numLines, 4);
            index.SetData(indexList);
            kernels[0] = shader.FindKernel("Run8");
            kernels[1] = shader.FindKernel("Run16");
        }
        
        ComputeBuffer filterCounts = new ComputeBuffer(1, 4);
        int[] fdat = { numLines };
        filterCounts.SetData(fdat);

        ComputeBuffer lines = index;
        int filterTilesX = 1;
        int filterTilesY = 1;
        for (int i =0; i<2; i++)
        {


            int kernel = kernels[i];
            shader.SetBuffer(kernel, "filterCounts", filterCounts);

            shader.SetBuffer(kernel, "buffer", buffer);
            shader.SetBuffer(kernel, "lines", lines);
            shader.SetBuffer(kernel, "counts", counts[i]);

            shader.SetInt("filterXtiles", filterTilesX);
            shader.SetInt("filterYtiles", filterTilesY);

            shader.SetInt("xtiles", (Screen.width / tileSize[i]));
            shader.SetInt("ytiles", (Screen.height / tileSize[i]));

            shader.SetInt("tileLines", tileLines[i]);
            shader.SetFloat("distCheck", Mathf.Sqrt(2) * tileSize[i] / Screen.width);
            print(kernel);
            shader.SetBuffer(kernel, "tiles", tiles[i]);
            
            uint x, y, z;

            shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);



            shader.Dispatch(kernel, Screen.width / (int)x / tileSize[i], Screen.height / (int)y / tileSize[i], 1);

            filterCounts = counts[i];
            filterTilesX = Screen.width / tileSize[i];
            filterTilesY = Screen.height / tileSize[i];
            lines = tiles[i];

        }

        
        
    }

    Material mat;


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Shader.SetGlobalBuffer("_Tiles", tiles[1]);
        Shader.SetGlobalBuffer("_Buffer", buffer);
        Shader.SetGlobalInt("_TileLines", tileLines[1]);
        Shader.SetGlobalBuffer("_Counts", counts[1]);
        Shader.SetGlobalInt("_Xtiles", (Screen.width / tileSize[1]));
        Shader.SetGlobalInt("_Ytiles", (Screen.height / tileSize[1]));
        Graphics.Blit(source, destination, mat);
    }

    /*
    public Mesh BuildMesh(int width, int height)
    {
        Mesh mesh = new Mesh();
        int tileSize = 16;
        int xtiles = width / tileSize;
        int ytiles = height / tileSize;
        int numVerts =  (xtiles + 1) * (ytiles + 1);
        Vector3[] verts = new Vector3[numVerts];

        int numTris = xtiles * ytiles * 6;

        int[] tris = new int[numTris];
        Vector2 ext = new Vector2(tileSize / (float)width, tileSize / (float)height);
        for(int x = 0; x<xtiles+1; x++)
        {
            for(int y=0; y<ytiles+1; y++)
            {
                verts[x + y * (xtiles+1)] = new Vector2(x, y) * ext;
            }
        }
        for (int x = 0; x < xtiles; x++)
        {
            for (int y = 0; y < ytiles; y++)
            {
                tris[(x + y * xtiles)*6] = x + y * (xtiles + 1);
                tris[(x + y*xtiles)*6+1] = x+1 + y * (xtiles + 1);
                tris[(x + y * xtiles) * 6 + 2] = x+1 + (y+1) * (xtiles + 1);

                tris[(x + y * xtiles) * 6 +3] = x + y * (xtiles + 1);
                tris[(x + y * xtiles) * 6 + 4] = x + 1 + (y + 1) * (xtiles + 1);
                tris[(x + y * xtiles) * 6 + 5] = x + (y+1) * (xtiles + 1);

            }
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        return mesh;
    }
    */

    // Update is called once per frame
    void Update()
    {
        
    }
}
