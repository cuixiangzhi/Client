#pragma once
#include "define/stdafx.h"

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
	inline thread_id get_tid() const { return m_tid; }
	inline bool get_active() const { return m_active; }
	inline THREAD_STATUS get_status() const { return m_status; }
	inline void set_status(THREAD_STATUS status) { m_status = status; }
	inline uint8 get_framerate() { return m_framerate; }
	inline void set_framerate(uint8 framerate) { m_framerate = framerate; }
	inline uint64 get_deltatime() { return m_deltatime; }
public: 
	virtual void init();
	virtual void loop();
	virtual void clear();
protected:
	thread_id m_tid;
	THREAD_STATUS m_status;
	bool m_active;

	uint8 m_framerate;
	uint64 m_framecount;

	uint64 m_deltatime;
	uint64 m_pre_frame_start_time;
	uint64 m_cur_frame_start_time;
#ifdef _WIN32
	HANDLE m_handle;
#endif
private:
	cthread() = delete;
	cthread(cthread&) = delete;
	cthread& operator=(cthread&) = delete;
};
