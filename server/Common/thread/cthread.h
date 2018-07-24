#pragma once
#include "define/common.h"

enum THREAD_STATUS
{
	READY		= 0,
	START		= 1,
	RUN			= 2,
	EXIT		= 3,
	DEAD		= 4,
};

struct THREAD_LOCK
{
#ifdef _WIN32
	CRITICAL_SECTION mutex;
#else
	pthread_mutex_t mutex;
#endif
	void* object;
	THREAD_LOCK* prev;
	THREAD_LOCK* next;
};

class cthread
{
public:
	cthread();
	virtual ~cthread();
public:
	void start();
	void stop();
	void exit();
public:
	virtual void init();
	virtual void loop();
	virtual void clear();
public:
	inline thread_fd id() const { return m_fd; }
	inline bool active() const { return m_active; }
	inline THREAD_STATUS status() const { return m_status; }
	inline void set_status(THREAD_STATUS status) { m_status = status; }
protected:
	void lock(void* object);
	void sleep(uint32 milliseconds);
protected:
	thread_fd m_fd;
	THREAD_STATUS m_status;
	THREAD_LOCK* m_locks;
#ifdef _WIN32
	HANDLE m_handle;
#endif	
	bool m_active;
private:
	cthread(cthread&) = delete;
	cthread& operator=(cthread&) = delete;
};
