#include "Server.h"
#include "iostream"

using namespace std;

void main()
{
	ReceiverSocket server(1000);
	server.Run();
}