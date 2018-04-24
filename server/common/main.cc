#include <winsock.h>


int main(int argc, char** argv)
{
	sockaddr_in addr;
	addr.sin_addr.s_addr = INADDR_ANY;
	addr.sin_family = AF_INET;
	addr.sin_port = htons(1);
}