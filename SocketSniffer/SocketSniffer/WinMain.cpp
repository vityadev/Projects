#include "StdAfx.h"
#include "Protocol.h"

#define PROMISC_MODE_ON 1
#define PROMISC_MODE_OFF 0
#define SIO_RCVALL 0x98000001

int mainSock;
sockaddr_in mainAddr;

#define PACKET_LENGTH 8192
byte buffer[PACKET_LENGTH];

int SetPromiscMode(DWORD *flag)
{
	if (ioctlsocket(mainSock, SIO_RCVALL, flag)) return 1;
	return 0;
}

int OpenSocket()
{
	WSADATA wsaData;
	if (WSAStartup(MAKEWORD(2, 2), &wsaData)) return 1;
	mainSock = socket(AF_INET, SOCK_RAW, IPPROTO_IP);
	if (mainSock == INVALID_SOCKET) return 1;
	char hostName[128];
	gethostname(hostName, sizeof(hostName));
	hostent *hostInfo = gethostbyname(hostName);
	mainAddr.sin_addr.s_addr = ((in_addr*)hostInfo->h_addr_list[0])->s_addr;
	mainAddr.sin_family = AF_INET;
	if (bind(mainSock, (sockaddr*)&mainAddr, sizeof(mainAddr))) return 1;
	return 0;
}

int CloseSocket()
{
	if (closesocket(mainSock)) return 1;
	if (WSACleanup()) return 1;
	return 0;
}

#define TCP_PROTOCOL 0x06
#define UDP_PROTOCOL 0x11

#define ULONG_BYTE4(u) ((u & 0xFF000000) >> 24)
#define ULONG_BYTE3(u) ((u & 0xFF0000) >> 16)
#define ULONG_BYTE2(u) ((u & 0xFF00) >> 8)
#define ULONG_BYTE1(u) (u & 0xFF)	

#define BYTE_L(u) (u & 0xF)
#define BYTE_H(u) (u >> 4)

#define IP_FLAGS(f) (f >> 13)
#define IP_OFFSET(o) (o & 0x1FFF)

char *Nethost2Str(DWORD host)
{
    const int BUFFER_LENGTH = 16;
	char *str = (char*)malloc(BUFFER_LENGTH);
	ZeroMemory(str, BUFFER_LENGTH);
	host = ntohl(host);
	sprintf(str, "%d.%d.%d.%d", ULONG_BYTE4(host), ULONG_BYTE3(host), ULONG_BYTE2(host), ULONG_BYTE1(host));
	return str;
}

uint32_t Iexp(uint32_t x, unsigned n)
{
	int p, y;
	y = 1;
	p = x;
	while(1)
	{
		if (n & 1) y *= p;
		n = n >> 1;
		if (n == 0)
			return y;
		p *= p;
	}
}

char *Int2Bin(uint16_t num, byte bits)
{
	char *strBin = (char*)malloc(bits+1);
	for(byte i = 0, m = Iexp(2, bits-1); m > 0; m /= 2, i++)
		strBin[i] = (num & m) ? '1' : '0';
	strBin[bits] = 0;
	return strBin;
}

char *IPPacket2Str(IPHeader *packet)
{
	const int BUFFER_LENGTH = 1024;
	char *str = (char*)malloc(BUFFER_LENGTH);
	ZeroMemory(str, BUFFER_LENGTH);

	char *t1 = Int2Bin(packet->iph_tos, 8);
	char *t2 = Int2Bin(IP_FLAGS(ntohs(packet->iph_flags)), 3);

	char *src = Nethost2Str(packet->iph_sourceip);
	char *dest = Nethost2Str(packet->iph_destip);

	sprintf(str, "ver=%d hlen=%d tos=%s len=%-4d id=%-5d flags=%s offset=%d ttl=%3dms prot=%-2d xsum=%-5X src=%-15s dest=%-15s\n",
		BYTE_H(packet->iph_ver), BYTE_L(packet->iph_ver)*4, t1, ntohs(packet->iph_len), ntohs(packet->iph_ident), 
		t2, IP_OFFSET(ntohs(packet->iph_offset)), packet->iph_ttl, packet->iph_protocol, 
		ntohs(packet->iph_xsum), src, dest);

	free(t1); free(t2); free(src); free(dest);

	return str;
}

IPHeader *SniffPacket()
{
	IPHeader *packet;
	int recvLength = recv(mainSock, (char*)&buffer[0], sizeof(buffer), 0);
	if (recvLength < sizeof(IPHeader)) return 0;
	packet = (IPHeader*)malloc(PACKET_LENGTH);
	memcpy(packet, buffer, PACKET_LENGTH);
	return packet;
}

void Split(vector<string> &vect, char *str, char *delim)
{
	char *pTempStr = strdup(str);
	char *pWord = strtok(pTempStr, delim);
	while(pWord != 0)
	{
		vect.push_back(pWord);
		pWord = strtok(0, delim);
	}
	free(pTempStr);
}

void main(int argc, char **argv)
{
	if (OpenSocket())
	{
		printf("OpenSocket error\n");
		exit(-1);
	}

	DWORD mode = PROMISC_MODE_ON;
	if (SetPromiscMode(&mode))
	{
		printf("SetPromiscMode error\n");
		exit(-1);
	}

	fstream file;
	file.open("C:\\Users\\tausvo\\Desktop\\byvitya\\SocketSniffer\\Debug\\packets.log", ios::out);
	while(1)
	{
		IPHeader *packet = SniffPacket();
		printf("writing packet...\n");

		/*TCPHeader *tcpPacket = (TCPHeader*)malloc(packet->iph_len);
		memcpy(tcpPacket, &packet->iph_data, packet->iph_len);

		byte *tcpData = (byte*)malloc(packet->iph_len);
		memcpy(tcpData, &tcpPacket->tcph_data, packet->iph_len);

		char *tcpStrData = (char*)malloc(packet->iph_len);
		memcpy(tcpStrData, &tcpData[128], packet->iph_len);
		printf(tcpStrData);

		free(tcpStrData);   error, rewrite this components
		free(tcpData);

		free(tcpPacket);
		//vector<string> splitedCoockies;*/

		char *str = IPPacket2Str(packet);
		file.write(str,strlen(str));
		free(str);

		free(packet);
	}
	file.close();
}

