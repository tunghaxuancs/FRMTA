#include "Server.h"

ReceiverSocket::ReceiverSocket(const int port_number){
	Port = (unsigned short)port_number;
	WSADATA wsaDataReci;
	int iResult = 0;
	iResult = WSAStartup(MAKEWORD(2, 2), &wsaDataReci);
	if (iResult != NO_ERROR) {
		wprintf(L"WSAStartup failed with error %d\n", iResult);
	}

	RecvSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (RecvSocket == INVALID_SOCKET) {
		wprintf(L"socket failed with error %d\n", WSAGetLastError());
		exit(0);
	}
	sockaddr_in RecvAddr;
	RecvAddr.sin_family = AF_INET;
	RecvAddr.sin_port = htons(Port);
	RecvAddr.sin_addr.s_addr = htonl(INADDR_ANY);

	iResult = bind(RecvSocket, (SOCKADDR *)& RecvAddr, sizeof(RecvAddr));
	if (iResult != 0) {
		wprintf(L"bind failed with error %d\n", WSAGetLastError());
	}

	std::cout << "Starting server on port " << port_number << std::endl;
}

void ReceiverSocket::GetPacket() {
	while (true)
	{
		sockaddr_in SenderAddr;
		int SenderAddrSize = sizeof(SenderAddr);

		int num_bytes = recvfrom(RecvSocket,
			RecvBuf, BufLen, 0, (SOCKADDR *)& SenderAddr, &SenderAddrSize);

		if (num_bytes == SOCKET_ERROR) {
			return;
		}

		wprintf(L"Receiving datagrams...\n");
		MessageData data(RecvBuf, num_bytes);

		incomingMessages.push(data);

		if (data.typeConnect == Login)
		{
			for (auto client : clients)
				if (client.first == data.clientName)
					client.second = SenderAddr;
		}

		Client client(data.clientName, SenderAddr);
		clients.insert(client);
	}
}
void ReceiverSocket::SendPacket(MessageData message, std::string nameClient){

	sockaddr_in RecvAddr;
	WSADATA wsaDataSend;
	for (auto client : clients)
		if (client.first == nameClient)
			RecvAddr = client.second;

	int iResult = WSAStartup(MAKEWORD(2, 2), &wsaDataSend);
	if (iResult != NO_ERROR) {
		wprintf(L"WSAStartup failed with error: %d\n", iResult);
		return;
	}

	SendSocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
	if (SendSocket == INVALID_SOCKET) {
		wprintf(L"socket failed with error: %ld\n", WSAGetLastError());
		return;
	}
	std::vector<unsigned char> data = message.toChar();

	int BufLen = data.size();
	iResult = sendto(SendSocket, (char*)data.data(), BufLen, 0, (SOCKADDR *)& RecvAddr, sizeof(RecvAddr));
	if (iResult == SOCKET_ERROR) {
		wprintf(L"sendto failed with error: %d\n", WSAGetLastError());
		closesocket(SendSocket);
		WSACleanup();
	}
}
void ReceiverSocket::Run()
{
	/*std::thread getPacket(&ReceiverSocket::GetPacket, this);
	getPacket.detach();*/
	std::thread processPacket(&ReceiverSocket::ProcessPacket, this);
	processPacket.detach();

	GetPacket();
}
void ReceiverSocket::ProcessPacket()
{
	int index = 0;
	while (true)
	{
		if (incomingMessages.empty()) continue;

		MessageData message = incomingMessages.pop();
		std::string path = "data\\face\\train";
		MessageData messageSend;
		switch (message.typeConnect)
		{
		case Login:
			messageSend.typeConnect = Login;
			message.clientName = message.clientName;
			SendPacket(messageSend, message.clientName);
			break;
		case Train:
			if (CreateDirectoryA((path + "\\" + message.clientName).c_str(), NULL) || ERROR_ALREADY_EXISTS == GetLastError())
			{
				std::cout << "Create folder success!" << std::endl;
				std::string temp = path + "\\" + message.clientName;
				cv::imwrite(temp + "\\" + message.clientName + "." + std::to_string(++index) + ".png", message.message);
			}
			else
			{
				std::cout << "Create folder error!" << std::endl;
			}
			break;
		case Predict:
			messageSend.typeConnect = Predict;
			std::cout << "Start predict...\n";
			messageSend.clientName = recognizor.Predict(message.message);

			std::cout << "Send result...\n";
			SendPacket(messageSend, message.clientName);
			break;
		default:
			break;
		}
	}
}


