#ifndef TESTS_CGINC
#define TESTS_CGINC

inline float det(float2 a, float2 b) { return (a.x*b.y-b.x*a.y); }// Find vector 𝑣𝑖 given pixel 𝑝=(0,0) and Bézier points 𝑏0,𝑏1,𝑏2. 
float2 get_distance_vector(float2 b0, float2 b1, float2 b2) 
{
    float a=det(b0,b2), b=2*det(b1,b0), d=2*det(b2,b1); 
    float f=b*d-a*a;
    float2 d21=b2-b1, d10=b1-b0, d20=b2-b0;
    float2 gf=2*(b*d21+d*d10+a*d20);
    gf=float2(gf.y,-gf.x);
    
    float2 pp=-f*gf/dot(gf,gf);
    
    float2 d0p=b0-pp;
    float ap=det(d0p,d20), bp=2*det(d10,d0p);
    
    // (note that 2*ap+bp+dp=2*a+b+d=4*area(b0,b1,b2))
    float t=clamp((ap+bp)/(2*a+b+d), 0,1);


    float2 dvec = lerp(lerp(b0,b1,t),lerp(b1,b2,t),t);

    return length(dvec);

} 

float2 get_line(float2 b0,float2 b1,  float2 b2){
    float2 v = b2-b0;
    float vlen = length(v);
    float2 vn = v/vlen;
    float2 vp = float2(vn.y,-vn.x);
    
    float dp = dot(vp,b0);
    float2 p = dp*vp;
    float t = saturate(dot(p-b0,vn)/vlen);

    return length(v*t+b0);

}

#endif
