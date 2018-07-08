#include <sys/socket.h>
#include <arpa/inet.h>
#include <netinet/in.h>
#include <stdio.h>
#include <unistd.h>
#include <errno.h>
#include <fcntl.h>
#include <string.h>

int main(int argc,char** argv)
{
	sockaddr_in server;
	memset(&server,0,sizeof(server));
	server.sin_family = AF_INET;
	server.sin_port = htons(5000);
	server.sin_addr.s_addr = inet_addr("123.114.147.57");

	int sock = socket(AF_INET,SOCK_STREAM,0);
	int flags = fcntl(sock,F_GETFL,0);
	fcntl(sock,F_SETFL,flags | O_NONBLOCK);

	int ret = connect(sock,(sockaddr*)&server,sizeof(sockaddr));
	if(ret == -1 )
	{
		printf("connect failed %d %d\n",ret,errno);
	}
	close(sock);
}
