#include <cstdio>

void* common_fopen(char* file,char* mode)
{
	return fopen(file,mode);
}

int common_fread(void* asset, int size, unsigned char* buffer)
{
	return (int)fread(buffer, size, 1, (FILE*)asset);
}

void common_fseek(void* asset, int offset, int where)
{
	fseek((FILE*)asset, offset, where);
}

void common_fclose(void* asset)
{
	fclose((FILE*)asset);
}
