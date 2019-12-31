using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kawase : MonoBehaviour
{
    Material tone;
    Material down;
    Material up;
    // Start is called before the first frame update
    void Start()
    {
        down = new Material(Shader.Find("Hidden/KawaseDown"));
        up = new Material(Shader.Find("Hidden/KawaseUp"));
        tone = new Material(Shader.Find("Hidden/ToneMap"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        int levels = 5;
        int backTo = 2;

        RenderTexture[] pym = new RenderTexture[levels];
        
        pym[0] = source;
        for(int i=1; i<levels; i++)
        {
            int pow = 2 << (i + 1);
            pym[i] = RenderTexture.GetTemporary(source.width / pow, source.height / pow, 0, source.format);
        }

        for(int i=1; i<levels; i++)
        {
            Graphics.Blit(pym[i-1], pym[i],down);
        }

        for(int i=levels-1; i>backTo; i--)
        {
            Graphics.Blit(pym[i], pym[i - 1], up);
        }

        Shader.SetGlobalTexture("_Bloom", pym[backTo]);
        Graphics.Blit(source, destination, tone);

        for(int i=1; i<levels; i++)
        {
            RenderTexture.ReleaseTemporary(pym[i]);
        }
    }
}
