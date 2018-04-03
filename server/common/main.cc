#if _MSC_VER
#include <windows.h>
#pragma comment(lib,"ws2_32.lib")
#else
#include <sys/socket.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#endif

#include <time.h>
#include <stdlib.h>
#include <stdio.h>

int main(int argc, char** argv)
{
#if _MSC_VER
	WORD word = MAKEWORD(2, 2);
	WSADATA lpsWSAData;
	if (WSAStartup(word, &lpsWSAData) != 0)
	{
		return -1;
	}
#endif
		
	fd_set fds;
	FD_ZERO(&fds);

	clock_t start = clock();
	for (int i = 0; i < 1000; i++)
	{
		if (FD_ISSET(i, &fds))
		{
			//printf("fd set %d", i);
		}
	}
	clock_t end = clock();
	printf("%d", end - start);
	system("pause");
}