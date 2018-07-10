#include "thread.h"

void thread_main_loop(void* param)
{
	try
	{
		thread* thread_object = reinterpret_cast<thread*>(param);
		thread_object->init();
		thread_object->set_status(THREAD_STATUS::RUN);
		while (thread_object->get_active())
		{
			thread_object->sleep();
			thread_object->loop();
		}
		thread_object->clear();
		thread_object->exit();
	}
	catch(...)
	{

	}
}
#ifdef _WIN32
DWORD WINAPI thread_main(void* param)
{
	thread_main_loop(param);
	return NULL;
}
#else
void* thread_main(void* param)
{
	thread_main_loop(param);
	return NULL;
}
#endif

thread::thread(uint64 framerate) :
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

thread::~thread()
{

}

void thread::start()
{
	if (m_status != THREAD_STATUS::READY)
		return;
	m_status = THREAD_STATUS::START;
#ifdef _WIN32
	m_handle = CreateThread(NULL, 0, thread_main, this, NULL, &m_tid);
#else
	m_tid = pthread_create(&m_tid, NULL, thread_main, this);
#endif
}

void thread::stop()
{
	m_active = false;
}

void thread::exit()
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

void thread::sleep()
{
	if (m_framecount != 0)
	{
		m_pre_frame_start_time = m_cur_frame_start_time;
		m_cur_frame_start_time = clock();
		m_deltatime = m_cur_frame_start_time - m_pre_frame_start_time;
	}
	else
	{
		m_cur_frame_start_time = clock();
	}
	++m_framecount;
}

void thread::init()
{

}

void thread::loop()
{
		
}

void thread::clear()
{

}


