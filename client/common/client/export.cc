#include "export.h"
#include "uuid/uuid32.h"

extern "C" int diff(char* oldpath, char* newpath, char* patchpath);
DLLAPI int common_diff(char* oldpath, char* newpath, char* patchpath)
{
	return diff(oldpath, newpath, patchpath);
}

extern "C"  int patch(char* oldpath, char* patchpath, char* newpath);
DLLAPI int common_patch(char* oldpath, char* patchpath, char* newpath)
{
	return patch(oldpath, patchpath, newpath);
}

DLLAPI void common_md5(char* data, int startIndex, char* outhash)
{
	_uuid_t uuid;
	char buffer[128];
	memset(buffer, '\0', sizeof(buffer));
	strcpy(buffer, data + startIndex);
	uuid_create_external(buffer, &uuid);
	char* hash = uuid_to_string(&uuid);
	strcpy(outhash, hash);
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

#ifdef __ANDROID__
static JavaVM* g_JavaVM = NULL;
JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void* reserved)
{
	g_JavaVM = vm;
}

AAssetManager* GetAssetManager(void* mgr)
{
	int status;
	JNIEnv* env = NULL;
	status = g_JavaVM->GetEnv((void **)&env, JNI_VERSION_1_6);

	if (status < 0)
	{
		status = g_JavaVM->AttachCurrentThread(&env, NULL);
		if (status < 0)
		{
			env = NULL;
		}
	}

	return AAssetManager_fromJava(env,(jobject)mgr);
}
#endif

DLLAPI void* common_android_open(char* file, void* mgr, int mode)
{
#ifdef __ANDROID__ 
	return (void*)AAssetManager_open(GetAssetManager(mgr), file, mode);
#else
	return NULL;
#endif
}

DLLAPI int common_android_read(void* asset, unsigned char* buffer, int len)
{
#ifdef __ANDROID__
	return AAsset_read((AAsset*)asset, (void*)buffer, len);
#else
	return 0;
#endif
}

DLLAPI void common_android_seek(void* asset, int offset, int where)
{
#ifdef __ANDROID__
	AAsset_seek((AAsset*)asset, offset, where);
#endif
}

DLLAPI void common_android_close(void* asset)
{
#ifdef __ANDROID__ 
	AAsset_close((AAsset*)asset);
#endif
}