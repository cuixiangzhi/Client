#pragma once
#ifdef _MSC_VER
#define DLLAPI __declspec(dllexport)
#else
#define DLLAPI
#endif

#ifdef __ANDROID__ 
#define VISIBILITY_FUNCTION __attribute__((visibility("default")))
#include "jni.h"
#include "android/asset_manager.h"
#include "android/asset_manager_jni.h"
#else
#define VISIBILITY_FUNCTION
#endif

extern "C"
{
	//二进制差分工具
	DLLAPI int common_diff(char* oldpath,char* newpath,char* patchpath) VISIBILITY_FUNCTION;
	DLLAPI int common_patch(char* oldpath, char* patchpath, char* newpath) VISIBILITY_FUNCTION;
	//加密解密UUID工具
	DLLAPI void common_md5(char* data, int startIndex, char* outhash) VISIBILITY_FUNCTION;
	DLLAPI void common_encode(unsigned char* data, int len) VISIBILITY_FUNCTION;
	DLLAPI void common_decode(unsigned char* data, int len) VISIBILITY_FUNCTION;
	//安卓IO工具
	DLLAPI void* common_android_open(char* file, void* mgr, int mode) VISIBILITY_FUNCTION;
	DLLAPI int common_android_read(void* asset,unsigned char* buffer, int len) VISIBILITY_FUNCTION;
	DLLAPI void common_android_seek(void* asset,int offset,int where) VISIBILITY_FUNCTION;
	DLLAPI void common_android_close(void* asset) VISIBILITY_FUNCTION;
};
