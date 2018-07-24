#include "cthread.h"

#define MAX_LOCK_COUNT 32

#ifdef _WIN32
DWORD WINAPI cthread_mainloop(void* param)
#else
void* cthread_mainloop(void* param)
#endif
{
	cthread* cthread_object = reinterpret_cast<cthread*>(param);
	cthread_object->init();
	cthread_object->set_status(THREAD_STATUS::RUN);
	while (cthread_object->active())
	{
		cthread_object->loop();
	}
	cthread_object->clear();
	cthread_object->exit();
}

cthread::cthread() :
	m_fd(0),
	m_status(THREAD_STATUS::READY),
	m_locks(NULL),
#ifdef _WIN32
	m_handle(NULL),
#endif
	m_active(true)
{

}

cthread::~cthread()
{

}

void cthread::start()
{
	if (m_status != THREAD_STATUS::READY)
		return;
	m_status = THREAD_STATUS::START;
#ifdef _WIN32
	m_handle = CreateThread(NULL, 0, cthread_mainloop, this, NULL, &m_fd);
#else
	pthread_create(&m_fd, NULL, cthread_mainloop, this);
#endif
}

void cthread::stop()
{
	m_active = false;
}

void cthread::exit()
{
	m_status = THREAD_STATUS::EXIT;
#ifdef _WIN32
	CloseHandle(m_handle);
	m_handle = NULL;
#else
	pthread_exit(NULL);
#endif
	m_status = THREAD_STATUS::DEAD;
}

void cthread::init()
{

}

void cthread::loop()
{

}

void cthread::clear()
{

}

void cthread::sleep(uint32 milliseconds)
{
#ifdef _WIN32
	Sleep(milliseconds);
#else
	usleep(milliseconds * 1000);
#endif
}

void cthread::lock(void* object)
{

}


