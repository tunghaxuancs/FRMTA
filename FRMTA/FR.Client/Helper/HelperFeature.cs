/******************************************************************************
*   by Ha Xuan Tung
*   Email: tung.haxuancs@gmail.com
******************************************************************************
*   Please don't clear this comments
*   Copyright MTA 2017.
*   Learn more in site: https://sites.google.com/site/ictw666/
*   Youtube channel: https://goo.gl/Caj8Gj
*****************************************************************************/

namespace FR.Client
{
    public enum AppMode
    {
        Predict,
        Register,
        Report,
        Login,
        Logout
    }

    public class HelperFeature
    {
        public const int DESIRED_CAMERA_WIDTH = 640;
        public const int DESIRED_CAMERA_HEIGHT = 480;

        public const double DESIRED_LEFT_EYE_X = 0.22;
        public const double DESIRED_LEFT_EYE_Y = 0.14;
        public const double FACE_ELLIPSE_CY = 0.40;
        public const double FACE_ELLIPSE_W = 0.50;
        public const double FACE_ELLIPSE_H = 0.80;

        public const int faceWidth = 90;
        public const int faceHeight = 90;

        public static string pathSaveImage = "data/face/";
        public static string pathSaveVideo = "data/video/";
        public static string pathLog = "data/log/";
        public static string pathLogApp = "data/log/app/";
        public static string pathLogSensor = "data/log/sensor/";
        public static string pathLogVideo = "data/log/video/";

        public static string ipServer = "127.0.0.1";
        public static int port = 1000;

        public static int Camera_Width = DESIRED_CAMERA_WIDTH;
        public static int Camera_Height = DESIRED_CAMERA_HEIGHT;
    }
}