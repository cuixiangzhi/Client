#include <arpa/inet.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdio.h>
#include <fcntl.h>
#include <errno.h>
#include <unistd.h>
#include <string.h>

int main(int argc,char** argv)
{
	sockaddr_in server;
	memset(&server,0,sizeof(server));
	server.sin_family = AF_INET;
	server.sin_port = htons(5000);
	server.sin_addr.s_addr = htonl(INADDR_ANY);

	int sock = socket(AF_INET,SOCK_STREAM,IPPROTO_TCP);
	bind(sock,(sockaddr*)&server,sizeof(server));
	listen(sock,SOMAXCONN);

	sockaddr_in client_addr;
	socklen_t size = sizeof(client_addr);

	while(true)
	{
		int client = accept(sock,(sockaddr*)&client_addr,&size);
		if(client < 0)
		{
			printf("receive connect request %d %d\n",client,errno);
		}
		else
		{
			printf("receive new socket %d",client);
			close(client);
		}	
	}
	close(sock);
}
