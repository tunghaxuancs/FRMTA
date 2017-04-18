/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/

using Emgu.CV;
using Emgu.CV.Structure;
using FR.Client.Scripts;
using FR.Client.Scripts.Networks;
using FR.Data;
using FR.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FR.Client.Moduls
{
    public partial class InsertDataFromPicture : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private Mat currentImage = new Mat();
        private string paths;
        private FaceDetectHaar fdh = new FaceDetectHaar();
        private CascadeClassifier cascade, eyeCascade1, eyeCascade2;

        private FaceDataRepositories faceRepo = new FaceDataRepositories();

        private Socket clientSocket;
        private string clientName;
        private EndPoint serverEndPoint;

        private byte[] dataSend = new byte[4096];

        public InsertDataFromPicture()
        {
            InitializeComponent();

            clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
            IPAddress ipAddress = IPAddress.Parse(HelperFeature.ipServer);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, HelperFeature.port);
            serverEndPoint = (EndPoint)ipEndPoint;
            clientName = "test";

            cascade = new CascadeClassifier("data/cascades/haarcascade_frontalface_default.xml");
            eyeCascade1 = new CascadeClassifier("data/cascades/haarcascade_eye.xml");
            eyeCascade2 = new CascadeClassifier("data/cascades/haarcascade_eye_tree_eyeglasses.xml");
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Chọn Ảnh";
            openFileDialog.Filter = "Image File|*.jpg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                paths = openFileDialog.FileName;
                currentImage = CvInvoke.Imread(paths, Emgu.CV.CvEnum.ImreadModes.Color);
                ibImage.Image = currentImage;

                LoadInfor();

                //Mat[] faceData = fdh.GetPreprocessedFace(currentImage,
                //          HelperFeature.faceWidth, HelperFeature.faceHeight,
                //          cascade, eyeCascade1, eyeCascade2, false, false);
                //for (int i = 0; i < faceData.Length; i++)
                //{
                //    Mat faceDataRec = new Mat(currentImage, fdh.faceRect[i]);
                //    faceDataRec.Save("avatar.png");
                //}
            }
        }

        private void LoadInfor()
        {
            if (!currentImage.IsEmpty)
            {
                Mat[] faceData = fdh.GetPreprocessedFace(currentImage,
                      HelperFeature.faceWidth, HelperFeature.faceHeight,
                      cascade, eyeCascade1, eyeCascade2, false, false);

                for (int i = 0; i < faceData.Length; i++)
                {
                    CvInvoke.Rectangle(currentImage, fdh.faceRect[i], new MCvScalar(255, 0, 0), 2);
                    ibImage.Image = currentImage;

                    RegisterCustomer frmText = new RegisterCustomer();
                    frmText.ShowDialog();
                    if (frmText.customer != null && frmText.customer.ID >= 0)
                    {
                        DataMessage msgToSend = new DataMessage();

                        msgToSend.clientName = frmText.customer.ID.ToString();
                        msgToSend.message = faceData[i];
                        msgToSend.typeConnect = TypeConnect.Train;

                        SendMessage(msgToSend);
                        string pathTemp = HelperFeature.pathSaveImage + frmText.customer.ID.ToString() + "/";
                        if (!Directory.Exists(pathTemp))
                        {
                            Directory.CreateDirectory(pathTemp);
                        }
                        Mat faceDataRec = new Mat(currentImage, fdh.faceRect[i]);
                        faceDataRec.Save(pathTemp + "avatar.png");
                        FaceData face = new FaceData();
                        face.CustomerID = frmText.customer.ID;
                        face.FaceImage = "avatar.png";
                        face.FaceFolder = pathTemp;
                        faceRepo.InsertData(face);
                    }
                    frmText.Dispose();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SendMessage(DataMessage msgToSend)
        {
            dataSend = new byte[2048];
            try
            {
                dataSend = msgToSend.ToByte();
                clientSocket.BeginSendTo(dataSend, 0, dataSend.Length, SocketFlags.None,
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
    }
}