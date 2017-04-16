using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FR.Client.Scripts
{
    public class FaceDetectHaar
    {
        private ObjectDetectHaar odh = new ObjectDetectHaar();

        private Rectangle storeFaceRect = new Rectangle(-1, -1, -1, -1);
        private Rectangle searchedLeftEye = new Rectangle(-1, -1, -1, -1);
        private Rectangle searchedRightEye = new Rectangle(-1, -1, -1, -1);

        private Point storeLeftEye = new Point(-1, -1);
        private Point storeRightEye = new Point(-1, -1);
        private Point leftEye = new Point(-1, -1);
        private Point rightEye = new Point(-1, -1);

        public string log = string.Empty;
        public Rectangle[] faceRect;
        private void InitVarible()
        {
            storeFaceRect = new Rectangle(-1, -1, -1, -1);
            searchedLeftEye = new Rectangle(-1, -1, -1, -1);
            searchedRightEye = new Rectangle(-1, -1, -1, -1);

            storeLeftEye = new Point(-1, -1);
            storeRightEye = new Point(-1, -1);
            leftEye = new Point(-1, -1);
            rightEye = new Point(-1, -1);
        }
        public Mat[] GetPreprocessedFace(Mat srcImg,
            int desiredFaceWidth, int desiredFaceHeight,
            CascadeClassifier faceCascade,
            CascadeClassifier eyeCascade1, CascadeClassifier eyeCascade2,
            bool doLeftAndRightSeparately, bool isCamera)
        {

            faceRect = null;
            faceRect = odh.DetectManyObjects(srcImg, faceCascade, HelperFeature.DESIRED_CAMERA_WIDTH, isCamera);
            List<Mat> faceData = new List<Mat>();
            if (faceRect == null) return faceData.ToArray();

            for (int i = 0; i < faceRect.Length; i++)
            {
                InitVarible();
                if (faceRect[i].Width > 0)
                {
                    storeFaceRect = faceRect[i];
                    Mat faceImg = new Mat(srcImg, faceRect[i]);


                    Mat gray = new Mat();
                    if (faceImg.NumberOfChannels == 3)
                    {
                        CvInvoke.CvtColor(faceImg, gray, ColorConversion.Bgr2Gray);
                    }
                    else if (faceImg.NumberOfChannels == 4)
                    {
                        CvInvoke.CvtColor(faceImg, gray, ColorConversion.Bgr2Gray);
                    }
                    else
                    {
                        gray = faceImg;
                    }

                    bool isDefaut = DetectBothEyes(gray, eyeCascade1, eyeCascade2, isCamera);

                    storeLeftEye = leftEye;
                    storeRightEye = rightEye;
                    if (leftEye.X >= 0 && rightEye.X >= 0)
                    {
                        PointF eyesCenter = new PointF((leftEye.X + rightEye.X) * 0.5f, (leftEye.Y + rightEye.Y) * 0.5f);

                        double dy = (rightEye.Y - leftEye.Y);
                        double dx = Math.Abs(rightEye.X - leftEye.X);
                        double len = Math.Sqrt(dx * dx + dy * dy);
                        double angle = Math.Atan2(dy, dx) * 180.0 / 3.14159265359;
                        if (isDefaut) angle = 0;

                        const double DESIRED_RIGHT_EYE_X = (1.0f - HelperFeature.DESIRED_LEFT_EYE_X);

                        double desiredLen = (DESIRED_RIGHT_EYE_X - HelperFeature.DESIRED_LEFT_EYE_X) * desiredFaceWidth;
                        double scale = desiredLen / len;

                        Mat rot_mat = new Mat();
                        CvInvoke.GetRotationMatrix2D(eyesCenter, angle, scale, rot_mat);

                        Image<Gray, double> temp_rot_mat = rot_mat.ToImage<Gray, double>();
                        temp_rot_mat.Data[0, 2, 0] += desiredFaceWidth * 0.5f - eyesCenter.X;
                        temp_rot_mat.Data[1, 2, 0] += desiredFaceHeight * HelperFeature.DESIRED_LEFT_EYE_Y - eyesCenter.Y;

                        rot_mat = temp_rot_mat.Mat;

                        Mat warped = new Mat(desiredFaceHeight, desiredFaceWidth, DepthType.Cv8U, 128);
                        CvInvoke.WarpAffine(gray, warped, rot_mat, warped.Size);

                        if (!doLeftAndRightSeparately)
                        {
                            CvInvoke.EqualizeHist(warped, warped);
                        }
                        else
                        {
                            warped = EqualizeLeftAndRightHalves(warped);
                        }

                        Mat filtered = new Mat(warped.Size, DepthType.Cv8U, 0);
                        CvInvoke.BilateralFilter(warped, filtered, 0, 10, 2.0);

                        Mat mask = warped;
                        mask = FilterMatContructor(mask);

                        Point faceCenter = new Point(desiredFaceWidth / 2, (int)Math.Round(desiredFaceHeight * HelperFeature.FACE_ELLIPSE_CY));
                        Size size = new Size((int)Math.Round(desiredFaceWidth * HelperFeature.FACE_ELLIPSE_W),
                            (int)Math.Round(desiredFaceHeight * HelperFeature.FACE_ELLIPSE_H));

                        CvInvoke.Ellipse(mask, faceCenter, size, 0, 0, 360, new MCvScalar(0));
                        mask = FilterEllipse(mask);

                        Mat dstImg = new Mat();
                        filtered.CopyTo(dstImg, mask);

                        faceData.Add(dstImg);

                        CvInvoke.Rectangle(srcImg, faceRect[i], new MCvScalar(0, 255, 0), 1);
                        //CvInvoke.Circle(face, leftEye, 2, new MCvScalar(0, 255, 0), 2);
                        //CvInvoke.Circle(face, rightEye, 2, new MCvScalar(0, 255, 0), 2);
                    }
                }
            }
            return faceData.ToArray();
        }

        private bool DetectBothEyes(Mat face, CascadeClassifier eyeCascade1, CascadeClassifier eyeCascade2, bool isCamera)
        {
            /*
	        // For "2splits.xml": Finds both eyes in roughly 60% of detected faces, also detects closed eyes.
	        const float EYE_SX = 0.12f;
	        const float EYE_SY = 0.17f;
	        const float EYE_SW = 0.37f;
	        const float EYE_SH = 0.36f;
	        */
            /*
            // For mcs.xml: Finds both eyes in roughly 80% of detected faces, also detects closed eyes.
            const float EYE_SX = 0.10f;
            const float EYE_SY = 0.19f;
            const float EYE_SW = 0.40f;
            const float EYE_SH = 0.36f;
            */

            // For default eye.xml or eyeglasses.xml: Finds both eyes in roughly 40% of detected faces, but does not detect closed eyes.
            const float EYE_SX = 0.16f;
            const float EYE_SY = 0.26f;
            const float EYE_SW = 0.32f;
            const float EYE_SH = 0.34f;

            int leftX = (int)Math.Round(face.Cols * EYE_SX);
            int topY = (int)Math.Round(face.Rows * EYE_SY);
            int widthX = (int)Math.Round(face.Cols * EYE_SW);
            int heightY = (int)Math.Round(face.Rows * EYE_SH);
            int rightX = (int)Math.Round(face.Cols * (1.0 - EYE_SX - EYE_SW));

            bool isDefaut = false;

            Mat topLeftOfFace = new Mat(face, new Rectangle(leftX, topY, widthX, heightY));
            Mat topRightOfFace = new Mat(face, new Rectangle(rightX, topY, widthX, heightY));
            Rectangle leftEyeRect, rightEyeRect;

            searchedLeftEye = new Rectangle(leftX, topY, widthX, heightY);
            searchedRightEye = new Rectangle(rightX, topY, widthX, heightY);

            leftEyeRect = odh.DetectLargestObject(topLeftOfFace, eyeCascade1, topLeftOfFace.Cols, !isCamera);
            rightEyeRect = odh.DetectLargestObject(topRightOfFace, eyeCascade1, topRightOfFace.Cols, !isCamera);

            Rectangle searchedLeftEyeTemp = searchedLeftEye;
            Rectangle searchedRightEyeTemp = searchedRightEye;

            if (leftEyeRect.Width <= 0)
            {
                leftEyeRect = odh.DetectLargestObject(topLeftOfFace, eyeCascade2, topLeftOfFace.Cols, !isCamera);

                if (leftEyeRect.Width > 0) log += DateTime.Now.ToString() + "2nd eye detector LEFT SUCCESS\n";
                else log += DateTime.Now.ToString() + "2nd eye detector LEFT FAILED\n";
            }
            if (rightEyeRect.Width <= 0)
            {
                rightEyeRect = odh.DetectLargestObject(topRightOfFace, eyeCascade2, topRightOfFace.Cols, !isCamera);
                if (rightEyeRect.Width > 0) log += DateTime.Now.ToString() + "2nd eye detector RIGHT SUCCESS";
                else log += DateTime.Now.ToString() + "2nd eye detector RIGHT FAILED\n";
            }

            if (leftEyeRect.Width <= 0)
            {
                leftEyeRect = new Rectangle(0, 0, searchedLeftEyeTemp.Height, searchedLeftEyeTemp.Width);
                isDefaut = true;
            }
            if (rightEyeRect.Width <= 0)
            {
                rightEyeRect = new Rectangle(0, 0, searchedRightEyeTemp.Height, searchedRightEyeTemp.Width);
                isDefaut = true;
            }

            if (leftEyeRect.Width > 0)
            {
                leftEyeRect.X += leftX;
                leftEyeRect.Y += topY;
                leftEye = new Point(leftEyeRect.X + leftEyeRect.Width / 2, leftEyeRect.Y + leftEyeRect.Height / 2);
            }
            else
            {
                leftEye = new Point(-1, -1);
            }

            if (rightEyeRect.Width > 0)
            {
                rightEyeRect.X += rightX;
                rightEyeRect.Y += topY;
                rightEye = new Point(rightEyeRect.X + rightEyeRect.Width / 2, rightEyeRect.Y + rightEyeRect.Height / 2);
            }
            else
            {
                rightEye = new Point(-1, -1);
            }

            return isDefaut;
        }

        private Mat EqualizeLeftAndRightHalves(Mat faceImg)
        {
            int w = faceImg.Cols;
            int h = faceImg.Rows;

            Mat wholeFace = new Mat();
            CvInvoke.EqualizeHist(faceImg, wholeFace);

            int midX = w / 2;
            Mat leftSide = new Mat(faceImg, new Rectangle(0, 0, midX, h));
            Mat rightSide = new Mat(faceImg, new Rectangle(midX, 0, w - midX, h));
            CvInvoke.EqualizeHist(leftSide, leftSide);
            CvInvoke.EqualizeHist(rightSide, rightSide);

            Image<Gray, byte> grayLeft = leftSide.ToImage<Gray, byte>();
            Image<Gray, byte> grayRight = rightSide.ToImage<Gray, byte>();
            Image<Gray, byte> grayFace = wholeFace.ToImage<Gray, byte>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int v;
                    if (x < w / 4)
                    {
                        v = grayLeft.Data[y, x, 0];
                    }
                    else if (x < w * 2 / 4)
                    {
                        int lv = grayLeft.Data[y, x, 0];
                        int wv = grayFace.Data[y, x, 0];

                        float f = (x - w * 1 / 4) / (float)(w * 0.25f);
                        v = (int)Math.Round((1.0f - f) * lv + (f) * wv);
                    }
                    else if (x < w * 3 / 4)
                    {
                        int rv = grayRight.Data[y, x - midX, 0];
                        int wv = grayFace.Data[y, x, 0];

                        float f = (x - w * 2 / 4) / (float)(w * 0.25f);
                        v = (int)Math.Round((1.0f - f) * wv + (f) * rv);
                    }
                    else
                    {
                        v = grayRight.Data[y, x - midX, 0];
                    }
                    grayFace.Data[y, x, 0] = (byte)v;
                }
            }
            return grayFace.Mat;
        }

        private Mat FilterMatContructor(Mat img)
        {
            if (img == null) return new Mat();

            Image<Gray, byte> gray = img.ToImage<Gray, byte>();
            for (int i = 0; i < gray.Rows; i++)
            {
                for (int j = 0; j < gray.Cols; j++)
                {
                    gray.Data[i, j, 0] = 255;
                }
            }
            return gray.Mat;
        }

        private Mat FilterEllipse(Mat img)
        {
            if (img == null) return new Mat();

            Image<Gray, byte> gray = img.ToImage<Gray, byte>();
            for (int i = 0; i < gray.Rows; i++)
            {
                bool isWhite = false;
                for (int j = 0; j < gray.Cols; j++)
                {
                    if (gray.Data[i, j, 0] == 0) isWhite = !isWhite;

                    if (gray.Data[i, j, 0] == 255 && !isWhite) gray.Data[i, j, 0] = 0;
                }
            }
            return gray.Mat;
        }
    }
}