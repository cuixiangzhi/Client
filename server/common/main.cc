#define _CRT_SECURE_NO_WARNINGS
#include <time.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <windows.h>
#include <io.h>
#include <fcntl.h>

int main(int argc, char** argv)
{
	HANDLE file = CreateFile("E:/a.txt", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, NULL, NULL);
	HANDLE handle = CreateFileMapping(file, NULL, PAGE_READWRITE, 0, 4, "hello_map");
	handle = OpenFileMapping(FILE_MAP_ALL_ACCESS, true, "hello_map");
	char* mem = (char*)MapViewOfFile(handle, FILE_MAP_ALL_ACCESS, 0, 0, 0);
	mem[1] = '5';
	UnmapViewOfFile(mem);
	CloseHandle(handle);
	CloseHandle(file);
	file = NULL;
	handle = NULL;
	mem = NULL;

	FILE* f = fopen("E:/a.txt", "rb");
	int err = GetLastError();
	fclose(f);

	int fd = _open("E:/a.txt", O_RDONLY);
	char buff[5];
	_read(fd, buff, sizeof(buff));
	_close(fd);

	system("pause");
}