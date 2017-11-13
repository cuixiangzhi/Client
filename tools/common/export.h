#pragma once
#ifdef _MSC_VER
#define DLLAPI __declspec(dllexport)
#else
#define DLLAPI
#endif

extern "C"
{
	DLLAPI int common_diff(char* oldpath,char* newpath,char* patchpath);
	DLLAPI int common_patch(char* oldpath, char* patchpath, char* newpath);
	DLLAPI void common_md5(char* data,char* outhash);
	DLLAPI void common_encode(unsigned char* data, int len);
	DLLAPI void common_decode(unsigned char* data, int len);
};
