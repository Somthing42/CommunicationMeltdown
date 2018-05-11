#if defined(SHADER_API_GLES2) || defined(SHADER_API_D3D9) || defined(SHADER_API_D3D11_9X)
half find_closest(half2 pos) {
	const int ditherPattern[9] = {
	 0, 7, 3,
	 6, 5, 2,
	 4, 1, 8};

	half2 positon = floor(pos * half2(screenWidth, screenHeight) % 3.0f);

	int dither = ditherPattern[int(positon.x) + int(positon.y) * 3];

	return half(dither) / 9.0f;
}
#else
inline half bayer2(half2 a){
    a = floor(a);
    return frac( a.x*.5+a.y*a.y*.75 );
}
#define bayer4(a)   (bayer2( .5*(a))*.25+bayer2(a))

half find_closest(half2 pos) {
	half2 p = pos * half2(screenWidth, screenHeight);
	return bayer4(p);
}
#endif

half series(half d, int N) {
	return frac(d + N * 0.5117);
}