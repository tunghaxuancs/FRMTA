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
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using FR.Client.Scripts;
using FR.Client.Scripts.Networks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;
using FR.Client.Moduls;
using FR.Repositories;
using System.Drawing.Imaging;
using FR.Data;

namespace FR.Client
{
    public partial class MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private VideoCapture camera;
        private Log appLog;
        private Mat currentCameraFrame;
        private Mat currentPredict;
        private string currentInfo;

        private Timer FPS;
        private int FPS_Count;

        private CascadeClassifier cascade, eyeCascade1, eyeCascade2;
        private FaceDetectHaar fdh;
        private ObjectDetectHaar odh;
        private Mat[] faceData;

        private BindingList<Record> listDataSource;

        private bool isSave;
        private int isLoadDataView;

        private Socket clientSocket;
        private string clientName;
        private EndPoint serverEndPoint;

        private byte[] dataRecieve = new byte[4096];
        private byte[] dataSend = new byte[4096];

        private int numRecord;

        private AppMode mode;
        private FaceDataRepositories faceRepo;
        private CustomerRepositories cusRepo;

        private VideoWriter videoW;

        public MainForm()
        {
            InitializeComponent();
            Init();
            InitDirectories();
            InitCascadeClassifier();
            InitCamera();
            InitConnectServer();
        }

        private void Init()
        {
            appLog = new Log();
            FPS = new Timer();
            FPS_Count = 0;

            FPS.Interval = 500;
            FPS.Tick += new EventHandler(FPS_Time_Tick);
            FPS.Start();

            fdh = new FaceDetectHaar();
            odh = new ObjectDetectHaar();

            listDataSource = new BindingList<Record>();
            gcDisplayDetect.DataSource = listDataSource;

            isSave = false;
            numRecord = 0;

            mode = AppMode.Predict;
            faceRepo = new FaceDataRepositories();
            cusRepo = new CustomerRepositories();

            videoW = new VideoWriter(HelperFeature.pathLogVideo + "\\" + DateTime.Now.ToFileName() + ".avi",
                                   16,
                                   new Size(HelperFeature.Camera_Width,
                                   HelperFeature.Camera_Height),
                                   true);
            isLoadDataView = 10;
        }

        private void InitDirectories()
        {
            if (!Directory.Exists(HelperFeature.pathSaveImage)) Directory.CreateDirectory(HelperFeature.pathSaveImage);
            if (!Directory.Exists(HelperFeature.pathSaveVideo)) Directory.CreateDirectory(HelperFeature.pathSaveVideo);
            if (!Directory.Exists(HelperFeature.pathLogApp)) Directory.CreateDirectory(HelperFeature.pathLogApp);
            if (!Directory.Exists(HelperFeature.pathLogSensor)) Directory.CreateDirectory(HelperFeature.pathLogSensor);
            if (!Directory.Exists(HelperFeature.pathLogVideo)) Directory.CreateDirectory(HelperFeature.pathLogVideo);
        }

        private void InitCamera()
        {
            Log sensorLog = new Log(false);
            try
            {
                camera = new VideoCapture();
                camera.SetCaptureProperty(CapProp.FrameWidth, HelperFeature.Camera_Width);
                camera.SetCaptureProperty(CapProp.FrameHeight, HelperFeature.Camera_Height);
                camera.Start();

                lbCameraStatus.Text = "Camera: Connected";
                lbCameraStatus.Appearance.ForeColor = Color.Green;
                lbStatusDisplayCamera.Text = "Connected";

                Application.Idle += new EventHandler(ProcessFrame);
                sensorLog.WriteLog("Connect Camera Success");
            }
            catch (Exception ex)
            {
                lbCameraStatus.Text = "No Connected";
                sensorLog.WriteLog("Connect Camera Error " + ex.Message);
            }
        }

        private void InitCascadeClassifier()
        {
            try
            {
                cascade = new CascadeClassifier("data/cascades/haarcascade_frontalface_default.xml");
                eyeCascade1 = new CascadeClassifier("data/cascades/haarcascade_eye.xml");
                eyeCascade2 = new CascadeClassifier("data/cascades/haarcascade_eye_tree_eyeglasses.xml");
            }
            catch (Exception ex)
            {
                appLog.WriteLog("Create CascadeClassifier Error " + ex.Message);
            }
        }

        private void InitConnectServer()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
            IPAddress ipAddress = IPAddress.Parse(HelperFeature.ipServer);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, HelperFeature.port);
            serverEndPoint = (EndPoint)ipEndPoint;
            clientName = (from nic in NetworkInterface.GetAllNetworkInterfaces()
                          where nic.OperationalStatus == OperationalStatus.Up
                          select nic.GetPhysicalAddress().ToString()).FirstOrDefault();

            DataMessage msgToSend = new DataMessage();
            msgToSend.clientName = this.clientName;
            msgToSend.message = null;
            msgToSend.typeConnect = TypeConnect.Login;
            SendMessage(msgToSend);
            ReceiveMessage();
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            currentCameraFrame = camera.QueryFrame();
            lbDateFrame.Text = DateTime.Now.ToString();
            FPS_Count++;

            Mat displayCameraFrame = new Mat();
            currentCameraFrame.CopyTo(displayCameraFrame);

            faceData = fdh.GetPreprocessedFace(displayCameraFrame,
                          HelperFeature.faceWidth, HelperFeature.faceHeight,
                          cascade, eyeCascade1, eyeCascade2, false, true);

            ibMain.Image = displayCameraFrame;

            ReceiveMessage();
            videoW.Write(displayCameraFrame);
        }

        private void FPS_Time_Tick(object sender, EventArgs e)
        {
            lbFPSCamera.Text = "FPS:" + FPS_Count.ToString();
            FPS_Count = 0;
            isLoadDataView++;

            if (fdh.faceRect != null && faceData != null && faceData.Length == fdh.faceRect.Length && mode != AppMode.Logout)
            {
                for (int i = 0; i < fdh.faceRect.Length; i++)
                {
                    lock (currentCameraFrame)
                    {
                        Mat face = new Mat(currentCameraFrame, fdh.faceRect[i]);
                        CvInvoke.Resize(face, face, new Size(HelperFeature.faceWidth, HelperFeature.faceHeight));

                        if (isSave)
                        {
                            face.Save(HelperFeature.pathSaveImage + DateTime.Now.ToFileTime() + ".png");
                        }

                        if (faceData[i] != null && mode == AppMode.Predict)
                        {
                            DataMessage msgToSend = new DataMessage();
                            msgToSend.clientName = this.clientName;
                            msgToSend.message = faceData[i];
                            msgToSend.typeConnect = TypeConnect.Predict;
                            SendMessage(msgToSend);

                            if (currentPredict == null) currentPredict = new Mat();
                            Record faceRecord = new Record(++numRecord, face.Bitmap, currentPredict.Bitmap, faceData[i].Bitmap, currentInfo);
                            if (listDataSource.Count > 100) listDataSource.RemoveAt(0);
                            listDataSource.Add(faceRecord);
                            currentInfo = string.Empty;
                            currentPredict = new Mat();
                        }
                        else continue;
                    }
                }
            }
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

        private void ReceiveMessage()
        {
            //dataRecieve = new byte[2048];
            try
            {
                clientSocket.BeginReceiveFrom(dataRecieve, 0, dataRecieve.Length, SocketFlags.None,
                ref serverEndPoint, new AsyncCallback(OnReceive), null);
            }
            catch
            {
                lbServerStatus.Text = "Server: Disconnect";
                lbServerStatus.Appearance.ForeColor = Color.Red;
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);

                DataMessage _msgReceived = new DataMessage(dataRecieve);
                switch (_msgReceived.typeConnect)
                {
                    case TypeConnect.Login:
                        lbServerStatus.Text = "Server: Connected";
                        lbServerStatus.Appearance.ForeColor = Color.Green;
                        break;

                    case TypeConnect.Logout:
                        lbServerStatus.Text = "Server: Disconnected";
                        lbServerStatus.Appearance.ForeColor = Color.Red;
                        mode = AppMode.Logout;
                        break;

                    case TypeConnect.Predict:
                        try
                        {
                            int.Parse(_msgReceived.clientName);
                            DataTable customer = cusRepo.SelectData("ID='" + _msgReceived.clientName + "'");
                            currentInfo = customer.Rows[0]["FullName"].ToString() + "\n\n" + customer.Rows[0]["Address"].ToString();
                            string pathTemp = HelperFeature.pathSaveImage + _msgReceived.clientName + "/avatar.png";
                            if (File.Exists(pathTemp)) currentPredict = CvInvoke.Imread(pathTemp, ImreadModes.Color);
                        }
                        catch { break; }
                        break;
                }
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                return;
            }
        }

        private void btnAppLog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FR.Client.Scripts.Log.Log dialog = new FR.Client.Scripts.Log.Log(FR.Client.Scripts.Log.TypeLog.app);
            dialog.ShowDialog();
        }

        private void btnSensorLog_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FR.Client.Scripts.Log.Log dialog = new FR.Client.Scripts.Log.Log(FR.Client.Scripts.Log.TypeLog.sensor);
            dialog.ShowDialog();
        }

        private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.HitInfo.InRow)
            {
                System.Drawing.Point p2 = Control.MousePosition;
                this.pmGcMain.ShowPopup(p2);
            }
        }

        private void btnRegister_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<Record> selectedRows = new List<Record>();
            foreach (int index in gridView1.GetSelectedRows())
            {
                Record task = gridView1.GetRow(index) as Record;
                if (task != null) selectedRows.Add(task);
            }
            if (selectedRows.Count == 0) MessageBox.Show("Please select row!", "Warning");
            else
            {
                mode = AppMode.Register;
                RegisterCustomer frmText = new RegisterCustomer();
                frmText.ShowDialog();
                if (frmText.customer != null && frmText.customer.ID >= 0)
                {
                    for (int i = 0; i < selectedRows.Count; i++)
                    {
                        Image<Gray, Byte> imageCV = new Image<Gray, byte>(selectedRows[i].PreProcessImg);
                        DataMessage msgToSend = new DataMessage();

                        msgToSend.clientName = frmText.customer.ID.ToString();
                        msgToSend.message = imageCV.Mat;
                        msgToSend.typeConnect = TypeConnect.Train;

                        SendMessage(msgToSend);
                    }
                    string pathTemp = HelperFeature.pathSaveImage + frmText.customer.ID.ToString() + "/";
                    if (!Directory.Exists(pathTemp))
                    {
                        Directory.CreateDirectory(pathTemp);
                    }
                    if (selectedRows.Count > 0)
                    {
                        selectedRows[0].DetectImg.Save(pathTemp + "avatar.png", ImageFormat.Png);
                        FaceData face = new FaceData();
                        face.CustomerID = frmText.customer.ID;
                        face.FaceImage = "avatar.png";
                        face.FaceFolder = pathTemp;
                        faceRepo.InsertData(face);
                    }
                }
                frmText.Dispose();
            }
            mode = AppMode.Predict;
        }

        private void btnReport_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            List<Record> selectedRows = new List<Record>();
            foreach (int index in gridView1.GetSelectedRows())
            {
                Record task = gridView1.GetRow(index) as Record;
                if (task != null) selectedRows.Add(task);
            }
            if (selectedRows.Count == 0) MessageBox.Show("Please select row", "Warning");
            else
            {
                mode = AppMode.Report;
                GetTextField frmText = new GetTextField();
                frmText.ShowDialog();
                if (!string.IsNullOrEmpty(frmText.TextField))
                {
                    for (int i = 0; i < selectedRows.Count; i++)
                    {
                        Image<Gray, Byte> imageCV = new Image<Gray, byte>(selectedRows[i].DetectImg);
                        DataMessage msgToSend = new DataMessage();
                        msgToSend.clientName = selectedRows[i].Info + "%" + frmText.TextField;
                        msgToSend.message = imageCV.Mat;
                        msgToSend.typeConnect = TypeConnect.Report;

                        SendMessage(msgToSend);
                    }
                }
                frmText.Dispose();
            }
            mode = AppMode.Predict;
        }

        private void gridView1_DataSourceChanged(object sender, EventArgs e)
        {
            showNewestRow();
        }

        private void gridView1_Click(object sender, EventArgs e)
        {
            isLoadDataView = 0;
        }

        private void barButtonItem12_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            InsertDataFromPicture temp = new InsertDataFromPicture();
            mode = AppMode.Logout;
            temp.ShowDialog();
            mode = AppMode.Predict;
        }

        private void showNewestRow()
        {
            if (gridView1.GetSelectedRows().Length > 0 || isLoadDataView < 10) return;

            int value = gridView1.RowCount - 1;
            gridView1.TopRowIndex = value;
            gridView1.FocusedRowHandle = value;
        }

        private void gridView1_RowCountChanged(object sender, EventArgs e)
        {
            showNewestRow();
        }
    }
}