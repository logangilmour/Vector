using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kawase : MonoBehaviour
{
    Material tone;
    Material down;
    Material up;
    Material feedback;
    RenderTexture[] lastFrame = new RenderTexture[2];

    // Start is called before the first frame update
    void Start()
    {
        down = new Material(Shader.Find("Hidden/KawaseDown"));
        up = new Material(Shader.Find("Hidden/KawaseUp"));
        tone = new Material(Shader.Find("Hidden/ToneMap"));
        feedback = new Material(Shader.Find("Hidden/Feedback"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    int frame = 0;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        

        for(int i=0; i<2; i++)
        {
            if (lastFrame[i] == null || !lastFrame[i].IsCreated())
            {
                lastFrame[i] = new RenderTexture(source.width, source.height, 0, source.format);
                lastFrame[i].Create();
            }
        }
            

        
        
        int levels = 6;
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
        Shader.SetGlobalTexture("_LastFrame", lastFrame[frame%2]);
        Graphics.Blit(source, lastFrame[(frame + 1) % 2], feedback);
        Shader.SetGlobalTexture("_Phosphorescence", lastFrame[(frame + 1) % 2]);
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
        frame++;
    }
}
