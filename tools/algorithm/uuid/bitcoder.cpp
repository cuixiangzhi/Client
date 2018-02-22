
static unsigned char magic_or_data[] = { 0xAB,0xCD,0xEF,0x12,0x34,0x56,0x78,0x9A };

void encrypt(unsigned char* data,int len)
{
	for (int i = 0; i < len; i++)
	{
		data[i] = data[i] ^ magic_or_data[i & 7];
	}
}

void decrypt(unsigned char* data, int len)
{
	for (int i = 0; i < len; i++)
	{
		data[i] = data[i] ^ magic_or_data[i & 7];
	}
}