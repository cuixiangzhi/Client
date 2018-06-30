#pragma once

#include <vector>
#include <list>
#include <map>
#include <hash_map>
#include <hash_set>

#include <cassert>
#include <cstdlib>
#include <cstdarg>
#include <cstdio>
#include <cstdint>
#include <cstring>
#include <ctime>
#include <cmath>

#include <cerrno>
#include <csignal>
#include <exception>
#include <csetjmp>

typedef void					VOID;
typedef char					CHAR;
typedef unsigned char			UCHAR;
typedef int						INT;
typedef unsigned int			UINT;
typedef short					SHORT;
typedef unsigned short			USHORT;
typedef float					FLOAT;
typedef unsigned char			BYTE;
typedef bool					BOOL;
typedef int64_t					INT64;
typedef uint64_t				UINT64;

#ifdef _MSC_VER
#include <windows.h>
#include <winsock.h>
typedef DWORD					THREAD_ID;
#else
#include <sys/ipc.h>
#include <sys/shm.h>

#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/epoll.h>

#include <pthread.h>
typedef pthread_t				THREAD_ID;
#endif