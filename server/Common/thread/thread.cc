#include "thread.h"

thread::thread() :m_active(false), m_status(THREAD_STATUS::READY), m_tid(0)
{
#ifdef _WIN32
	m_handle = NULL;
#endif
}

thread::~thread()
{

}

VOID thread::start()
{
#ifdef _WIN32
	m_handle = CreateThread(NULL, 0, loop_func, this, NULL, &m_tid);
#else
	m_tid = pthread_create(&m_tid, NULL, );
#endif
}

VOID thread::stop()
{
	m_active = false;
}

VOID thread::loop()
{

}

VOID thread::exit()
{

}

#ifdef _WIN32
DWORD WINAPI loop_func(VOID* param)
{

}
#else
VOID loop_func(VOID* param)
{

}
#endif


