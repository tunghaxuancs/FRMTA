using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Linq;
using System;
using Emgu.CV;

namespace FR.Client.Scripts.Networks
{
    public class ClientSide
    {
        private Socket clientSocket;
        private string clientName;
        private EndPoint serverEndPoint;

        byte[] data = new byte[2048];

        private bool isSend;
        private bool isReceive;
        private DataMessage _msgReceived;

        public DataMessage MsgReceived
        {
            get
            {
                if (isReceive) return _msgReceived;
                else return null;
            }
            set { _msgReceived = value; }
        }
        public ClientSide()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
            IPAddress ipAddress = IPAddress.Parse(HelperFeature.ipServer);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, HelperFeature.port);
            serverEndPoint = (EndPoint)ipEndPoint;
            clientName = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                          where nic.OperationalStatus == OperationalStatus.Up
                          select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
        }
        private void SendMessage(TypeConnect type, Mat message)
        {

            DataMessage msgToSend = new DataMessage();
            msgToSend.clientName = this.clientName;
            msgToSend.message = message;
            msgToSend.typeConnect = type;

            try
            {
                data = msgToSend.ToByte();
                clientSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None,
                    serverEndPoint, new AsyncCallback(OnSend), null);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Client Sending",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Client Sending ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void ConnectToServer()
        {
            SendMessage(TypeConnect.Login, null);
        }
        public void LogoutServer()
        {
            SendMessage(TypeConnect.Logout, null);
        }
        public void SendMessageForTrain(Mat message)
        {
            SendMessage(TypeConnect.Train, message);
        }
        public void SendMessageForPredict(Mat message)
        {
            SendMessage(TypeConnect.Predict, message);
        }
        private void ReceiveMessage()
        {
            data = new byte[2048];
            isReceive = false;

            try
            {
                clientSocket.BeginReceiveFrom(data, 0, data.Length, SocketFlags.None,
                ref serverEndPoint, new AsyncCallback(OnReceive), null);
            }
            catch(Exception ex)
            {
                isReceive = false;
                MessageBox.Show(ex.Message, "Receiving: " , MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);

                _msgReceived = new DataMessage(data);
                isReceive = true;
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                isReceive = false;
                MessageBox.Show(ex.Message, "Receiving", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}