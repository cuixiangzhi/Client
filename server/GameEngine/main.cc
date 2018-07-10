#include "thread/thread.h"

class test : public thread
{
public:
	virtual void loop()
	{
		int x = 0;
	}
};

int main(int argc,char** argv)
{
	thread* thread_object = new thread();
	thread_object->start();
	thread_object->stop();
	system("pause");
}