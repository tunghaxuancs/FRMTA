#pragma once
#include "iostream"
#include "opencv.hpp"

enum TypeConnect
{
	Login,
	Logout,
	Predict,
	Train,
	Message,
	Null,
	Report
};

class MessageData
{
	
public:
	MessageData();
	MessageData(char* data, int num_bytes);
	~MessageData();

	std::vector<unsigned char> toChar();

	TypeConnect typeConnect;
	std::string clientName;
	cv::Mat message;
};
