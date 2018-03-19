#ifdef __ANDROID__
#include "jni.h"
#include "android/asset_manager.h"
#include "android/asset_manager_jni.h"
#endif
#include <cstddef>

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

	return AAssetManager_fromJava(env, (jobject)mgr);
}
#endif

void* android_open(char* file, void* mgr, int mode)
{
#ifdef __ANDROID__ 
	return (void*)AAssetManager_open(GetAssetManager(mgr), file, mode);
#else
	return NULL;
#endif
}

int android_read(void* asset, unsigned char* buffer, int len)
{
#ifdef __ANDROID__
	return AAsset_read((AAsset*)asset, (void*)buffer, len);
#else
	return 0;
#endif
}

void android_seek(void* asset, int offset, int where)
{
#ifdef __ANDROID__
	AAsset_seek((AAsset*)asset, offset, where);
#endif
}

void android_close(void* asset)
{
#ifdef __ANDROID__ 
	AAsset_close((AAsset*)asset);
#endif
}