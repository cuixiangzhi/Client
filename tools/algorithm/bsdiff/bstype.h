#pragma once
#include <stdarg.h>
#include <assert.h>

#define err(...) assert(NULL)
#define errx(...) assert(NULL)
typedef int off_t_int;
typedef int ssize_t_int;
#define fseeko fseek
#define ftello ftell
