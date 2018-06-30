#pragma once
#include "define/type.h"

class thread
{
	enum THREAD_STATUS
	{
		READY,
		RUN,
		EXIT,
		STOP,
	};
public:
	thread();
	virtual ~thread();
public:
	VOID start();
	VOID stop();
private:
	virtual VOID loop();
	VOID exit();
public:
	THREAD_ID get_tid();
	BOOL get_active();
	THREAD_STATUS get_status();
private:
	THREAD_ID m_tid;
	BOOL m_active;
	THREAD_STATUS m_status;
#ifdef _WIN32
	HANDLE m_handle;
#endif

};
