#pragma once
#ifdef _MSC_VER
#define DLLAPI __declspec(dllexport)
#else
#define DLLAPI
#endif

#ifdef __ANDROID__ 
#define VISIBILITY_FUNCTION __attribute__((visibility("default")))
#else
#define VISIBILITY_FUNCTION
#endif

extern "C"
{
	DLLAPI int common_diff(char* oldpath,char* newpath,char* patchpath) VISIBILITY_FUNCTION;
	DLLAPI int common_patch(char* oldpath, char* patchpath, char* newpath) VISIBILITY_FUNCTION;
	DLLAPI void common_md5(char* data, int startIndex, char* outhash) VISIBILITY_FUNCTION;
	DLLAPI void common_encode(unsigned char* data, int len) VISIBILITY_FUNCTION;
	DLLAPI void common_decode(unsigned char* data, int len) VISIBILITY_FUNCTION;
};
