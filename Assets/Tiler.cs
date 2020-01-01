using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiler : MonoBehaviour
{
    public ComputeShader shader;
    public ComputeShader spiral;
    ComputeBuffer[] tiles = new ComputeBuffer[2];
    ComputeBuffer[] counts = new ComputeBuffer[2];
    ComputeBuffer buffer;
    ComputeBuffer index;
    Vector4[] data;
    int[] tileSize = {128,16 };
    const int reps = 2;
    
    int numLines = 50+3*reps+2*reps*4;
    int[] tileLines = { 500, 50 };
    int[] kernels = new int[2];
    bool demo = false;

    Vector2 ship = new Vector2(0, 0);
    float rotation = 0;

    Vector2 shipVel;
    Vector2 shipPos;

    double[] shipPath = { -0.5, -1, 0,1,
                           0,1, 0.5, -1,
                           0.5, -1, -0.5, -1};


    

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<2; i++)
        {
            tiles[i] = new ComputeBuffer((Screen.width / tileSize[i]) * (Screen.height / tileSize[i]) * tileLines[i], 4);
            counts[i] = new ComputeBuffer((Screen.width / tileSize[i]) * (Screen.height / tileSize[i]), 4);
        }
        mat = new Material(Shader.Find("Hidden/Bez"));

        wob = new Material(Shader.Find("Hidden/Wobble"));

        buffer = new ComputeBuffer(numLines, 4 * 4);


        //buffer.SetData(data);

        int[] indexList = new int[numLines];
        for (int i = 0; i < numLines; i++)
        {
            indexList[i] = i;
        }
        index = new ComputeBuffer(numLines, 4);
        index.SetData(indexList);
        kernels[0] = shader.FindKernel("Run8");
        kernels[1] = shader.FindKernel("Run16");

        data = new Vector4[numLines];
        Vector2 oldP = new Vector2(-1, -0.5f);
        for(int i=0; i<20; i++)
        {
            Vector2 newP = new Vector2((float)(i+1) / 20 * 2-1, oldP.y + Random.Range(-0.04f, 0.04f));
            data[i] = new Vector4(oldP.x, oldP.y, newP.x, newP.y);
            oldP = newP;
            oldP.x += 0.005f;
        }
        for(int i=20; i<50; i++)
        {
            Vector2 p = new Vector2(Random.Range(-1f, 1), Random.Range(-0.5f, 1));
            data[i] = new Vector4(p.x, p.y, p.x, p.y);
            if (i < 30)
            {
                i++;
                data[i] = new Vector4(p.x, p.y, p.x, p.y);
            }
        }
    }

    private void OnDestroy()
    {
        for (int i=0; i<2; i++)
        {
            tiles[i].Release();
            counts[i].Release();
            buffer.Release();
            index.Release();
        }
    }

    private void OnPreRender()
    {
        if (demo)
        {


            spiral.SetBuffer(0, "outbuf", buffer);
            uint xx, yy, zz;
            spiral.GetKernelThreadGroupSizes(0, out xx, out yy, out zz);
            spiral.SetFloat("time", Time.time);
            spiral.SetInt("numLines", numLines);
            spiral.Dispatch(0, numLines / (int)xx, 1, 1);

        }
        else
        {

            for(int i=0; i<shipPath.Length; i+=2)
            {
                Vector2 p = new Vector2((float)shipPath[i], (float)shipPath[i + 1]);
                p += Vector2.up * 0.5f;
                p = p * 0.04f;
                
                p = p.x * new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) + p.y * new Vector2(-Mathf.Sin(rotation), Mathf.Cos(rotation));

                p += shipPos;

                for (int j = 0; j < reps; j++)
                {
                    if (i % 4 == 0)
                    {
                        data[i / 4 + 50+j*shipPath.Length/4].x = p.x;
                        data[i / 4 + 50+j*shipPath.Length/4].y = p.y;
                    }
                    else
                    {
                        data[i / 4 + 50 + j * shipPath.Length / 4].z = p.x;
                        data[i / 4 + 50 + j * shipPath.Length / 4].w = p.y;
                    }
                }
            }
            for (int i = 0; i < shipPath.Length-4; i += 2)
            {
                if(i==1 || i == 2)
                {
                    
                }
                Vector2 p = new Vector2((float)shipPath[i], (float)shipPath[i + 1]);
                p.y = -p.y;
                if (i >=2 && i<= 5)
                {
                    p.y *= Random.value*10*(Input.GetKey(KeyCode.UpArrow)?1:0);
                }
                p -= Vector2.up*3;
                p = p * 0.01f;

                p = p.x * new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)) + p.y * new Vector2(-Mathf.Sin(rotation), Mathf.Cos(rotation));
                p += shipPos;
                for (int j = 0; j < reps*2; j++)
                {
                    if (i % 4 == 0)
                    {
                        data[i / 4 + 50 + reps*shipPath.Length / 4 + j * shipPath.Length / 4].x = p.x;
                        data[i / 4 + 50 + reps*shipPath.Length / 4 + j * shipPath.Length / 4].y = p.y;
                    }
                    else
                    {
                        data[i / 4 + 50 + reps*shipPath.Length / 4 + j * shipPath.Length / 4].z = p.x;
                        data[i / 4 + 50 + reps*shipPath.Length / 4 + j * shipPath.Length / 4].w = p.y;
                    }
                }
            }
            buffer.SetData(data);
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
            shader.SetBuffer(kernel, "tiles", tiles[i]);
            
            uint x, y, z;

            shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);



            shader.Dispatch(kernel, Screen.width / (int)x / tileSize[i], Screen.height / (int)y / tileSize[i], 1);

            if (i == 0)
            {
                filterCounts.Release();
            }
            filterCounts = counts[i];
            filterTilesX = Screen.width / tileSize[i];
            filterTilesY = Screen.height / tileSize[i];
            lines = tiles[i];

        }

        
        
    }

    Material mat;
    Material wob;


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Shader.SetGlobalBuffer("_Tiles", tiles[1]);
        Shader.SetGlobalBuffer("_Buffer", buffer);
        Shader.SetGlobalInt("_TileLines", tileLines[1]);
        Shader.SetGlobalBuffer("_Counts", counts[1]);
        Shader.SetGlobalInt("_Xtiles", (Screen.width / tileSize[1]));
        Shader.SetGlobalInt("_Ytiles", (Screen.height / tileSize[1]));
        var temp = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.RG16);
        Shader.SetGlobalFloat("_Wob", Random.value);
        Graphics.Blit(source, temp,wob);

        Shader.SetGlobalTexture("_Wobbler", temp);


        Graphics.Blit(source, destination, mat);

        RenderTexture.ReleaseTemporary(temp);
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
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rotation += Time.deltaTime;

        }else if (Input.GetKey(KeyCode.RightArrow))
        {
            rotation -= Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            shipVel += new Vector2(Mathf.Cos(rotation+Mathf.PI/2f), Mathf.Sin(rotation+Mathf.PI/2f)) * 0.003f;
        }

        shipVel += Vector2.down * 0.001f;
        shipPos += shipVel * Time.deltaTime;

    }
}
