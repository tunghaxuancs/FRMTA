#include "DataMessage.h"
#include "stdint.h"


MessageData::MessageData()
{
	typeConnect = Null;
	clientName = "";
	message = NULL;
}

MessageData::MessageData(char* data, int num_bytes)
{
	std::vector<unsigned char> img;
	if (num_bytes > 0) {
		typeConnect = (TypeConnect)(*reinterpret_cast<int32_t*>(data));
		int32_t lenName = *reinterpret_cast<int32_t*>(data + 4);
		int32_t lenData = *reinterpret_cast<int32_t*>(data + 8);

		if (lenName > 0)
		{
			for (size_t i = 0; i < lenName; i++)
			{
				clientName.push_back(data[12 + i]);
			}			
		}		
		img.insert(img.end(), &data[12 + lenName], &data[num_bytes]);
	}
	if (!img.empty())message = imdecode(img, cv::IMREAD_COLOR);
}
std::vector<unsigned char> intToBytes(int paramInt)
{
	std::vector<unsigned char> arrayOfByte(4);
	for (int i = 0; i < 4; i++)
		arrayOfByte[i] = (paramInt >> (i * 8));
	return arrayOfByte;
}

std::vector<unsigned char> MessageData::toChar()
{
	std::vector<unsigned char> img;

	std::vector<unsigned char> typeCon = intToBytes(typeConnect);
	for (size_t i = 0; i < typeCon.size(); i++)
		img.push_back(typeCon[i]);

	if (clientName.size() > 0)
	{
		std::vector<unsigned char> lenName = intToBytes(clientName.size());
		for (size_t i = 0; i < lenName.size(); i++)
		{
			img.push_back(lenName[i]);
		}
	}
	else
	{
		std::vector<unsigned char> lenName = intToBytes(0);
		for (size_t i = 0; i < lenName.size(); i++)
		{
			img.push_back(lenName[i]);
		}
	}
	if (!message.empty())
	{
		std::vector<unsigned char> lenData = intToBytes(message.rows*message.cols);
		for (size_t i = 0; i < lenData.size(); i++)
		{
			img.push_back(lenData[i]);
		}
	}
	else
	{
		std::vector<unsigned char> lenData = intToBytes(0);
		for (size_t i = 0; i < lenData.size(); i++)
		{
			img.push_back(lenData[i]);
		}
	}

	if (clientName.size() > 0)
	{
		std::vector<char> bytes(clientName.begin(), clientName.end());
		img.insert(img.end(), bytes.begin(), bytes.end());
	}

	if (!message.empty())
	{
		std::vector<unsigned char> data_buffer;
		const std::vector<int> compression_params = {
			cv::IMWRITE_JPEG_QUALITY, 100
		};
		cv::imencode(".jpg", message, data_buffer, compression_params);

		img.insert(img.end(), data_buffer.begin(), data_buffer.end());
	}

	return img;
}

MessageData::~MessageData()
{
}

