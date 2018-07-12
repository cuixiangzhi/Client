#pragma once

#include <vector>
#include <list>
#include <stack>
#include <queue>
#include <map>
#include <set>
#include <algorithm>
#include <numeric>

#include <cassert>
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <cstdlib>
#include <cstdarg>
#include <cstdio>
#include <ctime>
#include <cmath>

#include <cerrno>
#include <csignal>
#include <exception>
#include <csetjmp>
#include <atomic>

typedef unsigned char			byte;
typedef unsigned char			uchar;
typedef int8_t					int8;
typedef uint8_t					uint8;
typedef int16_t					int16;
typedef uint16_t				uint16;
typedef int32_t					int32;
typedef uint32_t				uint32;
typedef int64_t					int64;
typedef uint64_t				uint64;

#ifdef _WIN32
#include <windows.h>
#include <winsock.h>
typedef DWORD					thread_id;
#else
#include <unistd.h>

#include <sys/ipc.h>
#include <sys/shm.h>

#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#ifdef __APPLE__
#include <sys/select.h>
#else
#include <sys/epoll.h>
#endif

#include <pthread.h>
typedef pthread_t				thread_id;
#endif

using namespace std;
