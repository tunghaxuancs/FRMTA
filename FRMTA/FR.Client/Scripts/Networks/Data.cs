using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FR.Client.Scripts.Networks
{
    public enum TypeConnect
    {
        Login,
        Logout,
        Predict,
        Train,
        Message,
        Null, 
        Report
    }

    public class DataMessage
    {
        public TypeConnect typeConnect;
        public string clientName;
        public Mat message;

        public DataMessage()
        {
            typeConnect = TypeConnect.Null;
            clientName = string.Empty;
            message = null;
        }

        public DataMessage(byte[] data)
        {
            //the first four bytes are for type connect
            this.typeConnect = (TypeConnect)BitConverter.ToInt32(data, 0);

            //the next four store the length of name client
            int lengthName = BitConverter.ToInt32(data, 4);
            int lengthMessage = BitConverter.ToInt32(data, 8);

            //read name and message
            if (lengthName > 0) 
                this.clientName = Encoding.UTF8.GetString(data, 12, lengthName);
            else this.clientName = string.Empty;

            if (lengthMessage > 0) this.message = (new Bitmap(new MemoryStream(data.SubArray(12 + lengthName, lengthMessage)))).ToMat();
            else this.message = null;
        }
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();
            result.AddRange(BitConverter.GetBytes((int)typeConnect));

            if (clientName != null)
                result.AddRange(BitConverter.GetBytes(clientName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            VectorOfByte data = new VectorOfByte();
            if (message != null)
            {               
                CvInvoke.Imencode(".jpg", message, data);
                result.AddRange(BitConverter.GetBytes(data.Size));
            }
            else
                result.AddRange(BitConverter.GetBytes(0));

            if (clientName != null)
                result.AddRange(Encoding.UTF8.GetBytes(clientName));

            if (message != null)
            {
                result.AddRange(data.ToArray());
            }
            else
                result.AddRange(BitConverter.GetBytes(0));

            return result.ToArray();
        }
    }
}