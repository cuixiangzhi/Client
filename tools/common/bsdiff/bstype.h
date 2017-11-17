#pragma once
#include <stdarg.h>
#include <assert.h>

#define err(...) assert(NULL)
#define errx(...) assert(NULL)
typedef int off_t;
typedef int ssize_t;
#define fseeko fseek
#define ftello ftell
