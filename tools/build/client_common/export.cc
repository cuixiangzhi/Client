#include "export.h"

extern "C" int bsdiff(char* oldpath, char* newpath, char* patchpath);
DLLAPI int common_diff(char* oldpath, char* newpath, char* patchpath)
{
	return bsdiff(oldpath, newpath, patchpath);
}

extern "C"  int bspatch(char* oldpath, char* patchpath, char* newpath);
DLLAPI int common_patch(char* oldpath, char* patchpath, char* newpath)
{
	return bspatch(oldpath, patchpath, newpath);
}

extern void getmd5(char* data, int startIndex, char* outhash);
DLLAPI void common_md5(char* data, int startIndex, char* outhash)
{
	getmd5(data, startIndex, outhash);
}

extern void encrypt(unsigned char* data, int len);
DLLAPI void common_encode(unsigned char* data, int len)
{
	encrypt(data, len);
}

extern void decrypt(unsigned char* data, int len);
DLLAPI void common_decode(unsigned char* data, int len)
{
	decrypt(data, len);
}

extern void* android_open(char* file, void* mgr, int mode);
DLLAPI void* common_android_open(char* file, void* mgr, int mode)
{
	return android_open(file, mgr, mode);
}

extern int android_read(void* asset, unsigned char* buffer, int len);
DLLAPI int common_android_read(void* asset, unsigned char* buffer, int len)
{
	return android_read(asset, buffer, len);
}

extern void android_seek(void* asset, int offset, int where);
DLLAPI void common_android_seek(void* asset, int offset, int where)
{
	android_seek(asset, offset, where);
}

extern void android_close(void* asset);
DLLAPI void common_android_close(void* asset)
{
	android_close(asset);
}

extern void* common_fopen(char* file, char* mode);
DLLAPI void* common_open(char* file, char* mode)
{
	return common_fopen(file, mode);
}

extern int common_fread(void* asset, int size,  unsigned char* buffer);
DLLAPI int common_read(void* asset, int size, unsigned char* buffer)
{
	return common_fread(asset, size, buffer);
}

extern void common_fseek(void* asset, int offset, int where);
DLLAPI void common_seek(void* asset, int offset, int where)
{
	common_fseek(asset, offset, where);
}

extern void common_fclose(void* asset);
DLLAPI void common_close(void* asset)
{
	common_fclose(asset);
}

