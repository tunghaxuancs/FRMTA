/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/

#include "DataMessage.h"
#include "Recognizor.h"

#include <winsock2.h>
#include <Ws2tcpip.h>
#include <iostream>
#include "Queue.h"
#include <map>
#include <thread>

#pragma comment(lib, "Ws2_32.lib")

typedef std::map<std::string, sockaddr_in> ClientList;
typedef ClientList::value_type Client;

class ReceiverSocket {
public:

	ReceiverSocket(const int port_number);
	void Run();

private:

	SOCKET RecvSocket;
	SOCKET SendSocket = INVALID_SOCKET;

	unsigned short Port = 1000;

	char RecvBuf[20000];
	int BufLen = 20000;

	LockedQueue<MessageData> incomingMessages;
	ClientList clients;

	std::vector<KAZERecognizor> models;
	std::vector<std::string> directories;

	void GetPacket();
	void SendPacket(MessageData message, std::string nameClient);
	void ProcessPacket();
	void InitModel();
};
