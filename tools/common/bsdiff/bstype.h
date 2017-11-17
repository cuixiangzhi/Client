#pragma once
#include <stdarg.h>
#include <assert.h>

#define err(...) asset(NULL)
#define errx(...) asset(NULL)
typedef int off_t;
typedef int ssize_t;
#define fseeko fseek
#define ftello ftell
