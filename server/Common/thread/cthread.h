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

class cthread
{
public:
	cthread(uint8 framerate);
	virtual ~cthread();
public:
	void start();
	void stop();
	void exit();
	void sleep();
public:
	inline thread_fd id() const { return m_fd; }
	inline bool active() const { return m_active; }
	inline THREAD_STATUS status() const { return m_status; }
	inline uint8 framerate() { return m_framerate; }
	inline uint64 deltatime() { return m_deltatime; }
	inline void set_status(THREAD_STATUS status) { m_status = status; }
	inline void set_framerate(uint8 framerate) { m_framerate = framerate; }
public: 
	virtual void init();
	virtual void loop();
	virtual void clear();
protected:
	thread_fd m_fd;
	THREAD_STATUS m_status;

	uint8 m_framerate;
	uint64 m_framecount;

	uint64 m_deltatime;
	uint64 m_pre_frame_start_time;
	uint64 m_cur_frame_start_time;
#ifdef _WIN32
	HANDLE m_handle;
#endif
	bool m_active;
private:
	cthread() = delete;
	cthread(cthread&) = delete;
	cthread& operator=(cthread&) = delete;
};
