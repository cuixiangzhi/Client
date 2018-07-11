#include "cthread.h"

void cthread_main_loop(void* param)
{
	try
	{
		cthread* cthread_object = reinterpret_cast<cthread*>(param);
		cthread_object->init();
		cthread_object->set_status(THREAD_STATUS::RUN);
		while (cthread_object->get_active())
		{
			cthread_object->sleep();
			cthread_object->loop();
		}
		cthread_object->clear();
		cthread_object->exit();
	}
	catch(...)
	{

	}
}
#ifdef _WIN32
DWORD WINAPI cthread_main(void* param)
{
	cthread_main_loop(param);
	return NULL;
}
#else
void* cthread_main(void* param)
{
	cthread_main_loop(param);
	return NULL;
}
#endif

cthread::cthread(uint8 framerate) :
	m_tid(0),
	m_status(THREAD_STATUS::READY),
	m_active(true),
	m_framerate(framerate),
	m_framecount(0),
	m_deltatime(0),
	m_pre_frame_start_time(0),
	m_cur_frame_start_time(0)
{
#ifdef _WIN32
	m_handle = NULL;
#endif
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
	m_handle = CreateThread(NULL, 0, cthread_main, this, NULL, &m_tid);
#else
	m_tid = pthread_create(&m_tid, NULL, cthread_main, this);
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

void cthread::sleep()
{
	if (m_framecount != 0)
	{
		m_pre_frame_start_time = m_cur_frame_start_time;
		m_cur_frame_start_time = clock();
		m_deltatime = m_cur_frame_start_time - m_pre_frame_start_time;
		++m_framecount;
	}
	else
	{
		m_cur_frame_start_time = clock();
		++m_framecount;
	}
#ifdef _WIN32
	Sleep(1000 / m_framerate);
#else
	usleep(1000000 / m_framerate);
#endif
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


