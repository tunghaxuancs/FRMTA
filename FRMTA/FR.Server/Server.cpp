/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/
#include "Server.h"
#include <experimental/filesystem>

inline std::string getName(const std::string& filename)
{
	return filename.substr(filename.find_last_of('\\') + 1);
}

std::string wchar2string(const wchar_t *wchar)
{
	std::string str = "";
	int index = 0;
	while (wchar[index] != 0)
	{
		str += (char)wchar[index];
		++index;
	}
	return str;
}

wchar_t *string2wchar(const std::string &str)
{
	wchar_t wchar[260];
	int index = 0;
	while (index < str.size())
	{
		wchar[index] = (wchar_t)str[index];
		++index;
	}
	wchar[index] = 0;
	return wchar;
}

std::vector<std::string> getDirectory(const std::string& directory)
{
	std::string search_path = directory + "/*.*";
	WIN32_FIND_DATA FindFileData;
	wchar_t * FileName = string2wchar(search_path);
	HANDLE hFind = FindFirstFile(FileName, &FindFileData);
	std::vector<std::string> listFileNames;
	if (hFind != INVALID_HANDLE_VALUE)
	{
		do {
			if ((FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)) {
				listFileNames.push_back(wchar2string(FindFileData.cFileName));
			}
		} while (::FindNextFile(hFind, &FindFileData));
		::FindClose(hFind);
	}
	return listFileNames;
}
ReceiverSocket::ReceiverSocket(const int port_number) {
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
void ReceiverSocket::SendPacket(MessageData message, std::string nameClient) {
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
	std::thread getPacket(&ReceiverSocket::GetPacket, this);
	getPacket.detach();
	/*std::thread processPacket(&ReceiverSocket::ProcessPacket, this);
	processPacket.detach();*/
	InitModel();
	ProcessPacket();
}

void ReceiverSocket::ProcessPacket()
{
	int index = 0;

	while (true)
	{
		if (incomingMessages.empty()) continue;
		MessageData message = incomingMessages.pop();
		std::string pathTrain = "data\\face\\train";
		std::string pathReport = "data\\face\\report";
		MessageData messageSend;
		float maxPredict = -1;
		int maxIndex = 0;
		std::string infor;

		switch (message.typeConnect)
		{
		case Login:
			messageSend.typeConnect = Login;
			message.clientName = message.clientName;
			SendPacket(messageSend, message.clientName);
			break;
		case Train:
			if (CreateDirectoryA((pathTrain + "\\" + message.clientName).c_str(), NULL) || ERROR_ALREADY_EXISTS == GetLastError())
			{
				std::cout << "Create folder success!" << std::endl;
				std::string temp = pathTrain + "\\" + message.clientName;
				cv::imwrite(temp + "\\" + message.clientName + "." + std::to_string(++index) + ".png", message.message);
			}
			else
			{
				std::cout << "Create folder error!" << std::endl;
			}
			break;
		case Predict:
			messageSend.typeConnect = Predict;
			if (models.size() == 0) continue;
			for (int i = 0;i < directories.size();i++)
			{
				KAZERecognizor kaze(directories.at(i));
				float temp = kaze.Predict(message.message);
				if (temp > maxPredict)
				{
					maxPredict = temp;
					maxIndex = i;
				}
			}

			messageSend.clientName = getName(directories.at(maxIndex));
			SendPacket(messageSend, message.clientName);
			break;
		case Report:
			infor = message.clientName.substr(message.clientName.find_first_of('%') + 1);
			if (CreateDirectoryA((pathReport + "\\" + infor).c_str(), NULL) || ERROR_ALREADY_EXISTS == GetLastError())
			{
				std::cout << "Create folder report success!" << std::endl;
				std::string temp = pathReport + "\\" + infor;
				cv::imwrite(temp + "\\report.png", message.message);

				std::ofstream reportText(temp + "\\report.txt");
				reportText << message.clientName;
				reportText.close();
			}
			else
			{
				std::cout << "Create folder error!" << std::endl;
			}
			break;
		default:
			break;
		}
	}
}

void ReceiverSocket::InitModel()
{
	std::string path = "data/face/data";

	/*std::experimental::filesystem::path path(pathString);*/
	for (auto & p : std::experimental::filesystem::directory_iterator(path))
	{
		std::string rootPath = p.path().string();
		directories.push_back(rootPath);
		KAZERecognizor kaze(rootPath);
		models.push_back(kaze);
	}
}