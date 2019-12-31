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
        int levels = 7;
        int backTo = 1;

        RenderTexture[] pym = new RenderTexture[levels];
        RenderTexture[] pymup = new RenderTexture[levels];
        pym[0] = source;
        for(int i=1; i<levels; i++)
        {
            int pow = 2 << (i);
            pym[i] = RenderTexture.GetTemporary(source.width / pow, source.height / pow, 0, source.format);

            if (i < levels - 1)
            {
                pymup[i]= RenderTexture.GetTemporary(source.width / pow, source.height / pow, 0, source.format);
            }
            else
            {
                pymup[i] = pym[i];
            }
        }

        for(int i=1; i<levels; i++)
        {
            Graphics.Blit(pym[i-1], pym[i],down);
        }

        for(int i=levels-1; i>backTo; i--)
        {
            Shader.SetGlobalTexture("_CurTex", pym[i]);
            Graphics.Blit(pymup[i], pymup[i - 1], up);
        }

        Shader.SetGlobalTexture("_Bloom", pymup[backTo]);
        Graphics.Blit(source, destination, tone);

        for(int i=1; i<levels; i++)
        {
            RenderTexture.ReleaseTemporary(pym[i]);
            if (i < levels - 1)
            {
                RenderTexture.ReleaseTemporary(pymup[i]);
            }
        }
    }
}
