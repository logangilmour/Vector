#ifndef TESTS_CGINC
#define TESTS_CGINC

float get_line(float2 b0,float2 b1,  float2 b2){
    float2 v = b2-b0;
    float vlen = length(v);
    float2 vn = v/vlen;
    float2 vp = float2(vn.y,-vn.x);
    
    float dp = dot(vp,b0);
    float2 p = dp*vp;
    float t = saturate(dot(p-b0,vn)/vlen);

    return length(v*t+b0);

}

inline float det(float2 a, float2 b) { return (a.x*b.y-b.x*a.y); }// Find vector 𝑣𝑖 given pixel 𝑝=(0,0) and Bézier points 𝑏0,𝑏1,𝑏2. 

float2 subvec(float2 b0, float2 b1, float2 b2) 
{
    float a=det(b0,b2), b=2*det(b1,b0), d=2*det(b2,b1); 
    float f=b*d-a*a;
    float2 d21=b2-b1, d10=b1-b0, d20=b2-b0;
    float2 gf=2*(b*d21+d*d10+a*d20);
    gf=float2(gf.y,-gf.x);

    float2 b3 = b0+b2;

    float vt = dot(b0-b1,b3-2*b1)/(dot(b3,b3)-dot(4*b1,b3-b1));
    
    float2 pp=-f*gf/dot(gf,gf);
    
    float2 d0p=b0-pp;
    float ap=det(d0p,d20), bp=2*det(d10,d0p);
    
    // (note that 2*ap+bp+dp=2*a+b+d=4*area(b0,b1,b2))
    float t=clamp((ap+bp)/(2*a+b+d), 0,1);


    float2 dvec = lerp(lerp(b0,b1,t),lerp(b1,b2,t),t);
    float2 vp = lerp(lerp(b0,b1,vt),lerp(b1,b2,vt),vt);
    float o = length(dvec);
    //o = min(o,max(0,length(vp)-0.003));

    float ln = get_line(b1,float2(0,0),(b0+b2)/2);

    //o = min(ln,o);

    for(int i=0; i<0; i++){
        t = (float)i/10.0;
        float ball = max(0,length(lerp(lerp(b0,b1,t),lerp(b1,b2,t),t))-0.003);
        o = min(o,ball);
    }
    return o;

} 

float search_distance_pnt(float2 b0, float2 b1, float2 b2){
    float t=0.5;
    float p = 0.5;
    for(int i=0; i<6; i++){
        p = p*0.5;
        float2 v0 = lerp(b0,b1,t);
        float2 v1 = lerp(b1,b2,t);
        float2 dir = v0-v1;
        float2 pos = lerp(v0,v1,t);
        t+= p*sign(dot(pos,dir));
    }
    t = clamp(t,0,1);
    return length(lerp(lerp(b0,b1,t),lerp(b1,b2,t),t));
}

float search_distance(float2 b0, float2 b1, float2 b2){
    float t=0.5;
    float p = 0.5;

    float2 start = b0;
    float2 end = b2;


    for(int i=0; i<6; i++){
        p = p*0.5;
        float2 v0 = lerp(b0,b1,t);
        float2 v1 = lerp(b1,b2,t);
        float2 dir = v0-v1;
        float2 pdir;
        pdir.x = dir.y;
        pdir.y = -dir.x;
        float2 pos = lerp(v0,v1,t);
        t+= p*sign(dot(pos,dir));
    }
    t = clamp(t,0,1);
    float2 v0 = lerp(b0,b1,t);
    float2 v1 = lerp(b1,b2,t);
    float2 v = v1-v0;
    float vlen = length(v);
    float2 vn = v/vlen;
    float2 vp = float2(vn.y,-vn.x);
    
    float dp = dot(vp,v0);
    float2 p2 = dp*vp;
    float t2 = dot(p2-v0,vn)/vlen;
    t2 = clamp(t2,-p,1+p);
    return length(v*t2+v0);

}


float get_distance_vector(float2 b0, float2 b1, float2 b2) 
{
    float2 b3 = b0+b2;

    float vt = dot(b0-b1,b3-2*b1)/(dot(b3,b3)-dot(4*b1,b3-b1));
    
    float2 v0 = lerp(b0,b1,vt);
    float2 v1 = lerp(b1,b2,vt);

    float2 vp = lerp(v0,v1,vt);
    float o = min(search_distance(b0,v0,vp),search_distance(b2,v1,vp));
    return o;

} 



#endif
