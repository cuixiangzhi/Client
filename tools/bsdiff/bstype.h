#pragma once
#include <stdarg.h>
#include <assert.h>

#define err(...) assert(NULL)
#define errx(...) assert(NULL)
typedef int off_t_int;
typedef int ssize_t_int;
#define fseeko fseek
#define ftello ftell

#ifdef _MSC_VER
#define BSAPI __declspec(dllexport)
#else
#define BSAPI
#endif

extern "c"
{
	BSAPI int bsdiff(char* oldpath, char* newpath, char* patchpath);
	BSAPI int bspatch(char* oldpath, char* patchpath, char* newpath);
};
