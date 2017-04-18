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
using System;
using System.Drawing;

namespace FR.Client.Scripts
{
    public class ObjectDetectHaar
    {
        private Rectangle[] objects;

        private void DetectObjectsCustom(Mat img, CascadeClassifier cascade, int scaledWidth, Size minFeatureSize, float searchScaleFactor, int minNeighbors)
        {
            Mat gray = new Mat();
            if (img.NumberOfChannels == 3)
            {
                CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            }
            else if (img.NumberOfChannels == 4)
            {
                CvInvoke.CvtColor(img, gray, ColorConversion.Bgr2Gray);
            }
            else
            {
                gray = img;
            }
            Mat inputImg = new Mat();
            float scale = img.Cols / (float)scaledWidth;
            if (img.Cols > scaledWidth)
            {
                int scaledHeight = (int)Math.Round(img.Rows / scale);
                CvInvoke.Resize(gray, inputImg, new Size(scaledWidth, scaledHeight));
            }
            else
            {
                inputImg = gray;
            }

            Mat equalizedImg = new Mat();
            CvInvoke.EqualizeHist(inputImg, equalizedImg);

            objects = cascade.DetectMultiScale(gray, searchScaleFactor, minNeighbors, minFeatureSize);

            if (img.Cols > scaledWidth)
            {
                for (int i = 0; i < (int)objects.Length; i++)
                {
                    objects[i].X = (int)Math.Round(objects[i].X * scale);
                    objects[i].Y = (int)Math.Round(objects[i].Y * scale);
                    objects[i].Width = (int)Math.Round(objects[i].Width * scale);
                    objects[i].Height = (int)Math.Round(objects[i].Height * scale);
                }
            }

            for (int i = 0; i < (int)objects.Length; i++)
            {
                if (objects[i].X < 0)
                    objects[i].X = 0;
                if (objects[i].Y < 0)
                    objects[i].Y = 0;
                if (objects[i].X + objects[i].Width > img.Cols)
                    objects[i].X = img.Cols - objects[i].Width;
                if (objects[i].Y + objects[i].Height > img.Rows)
                    objects[i].Y = img.Rows - objects[i].Height;
            }
        }

        public Rectangle DetectLargestObject(Mat img, CascadeClassifier cascade, int scaledWidth, bool isCamera)
        {
            Size minFeatureSize;
            if (isCamera) minFeatureSize = new Size(70, 70);
            else minFeatureSize = new Size(20, 20);
            float searchScaleFactor = 1.1f;
            int minNeighbors = 5;

            DetectObjectsCustom(img, cascade, scaledWidth, minFeatureSize, searchScaleFactor, minNeighbors);
            if (objects.Length > 0)
            {
                Rectangle largestObject = new Rectangle(-1, -1, -1, -1);
                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i].Width * objects[i].Height > largestObject.Width * largestObject.Height) largestObject = objects[i];
                }
                return largestObject;
            }
            else return new Rectangle(-1, -1, -1, -1);
        }

        public Rectangle[] DetectManyObjects(Mat img, CascadeClassifier cascade, int scaledWidth, bool isCamera)
        {
            Size minFeatureSize;
            if (isCamera) minFeatureSize = new Size(70, 70);
            else minFeatureSize = new Size(20, 20);
            float searchScaleFactor = 1.1f;
            int minNeighbors = 5;

            DetectObjectsCustom(img, cascade, scaledWidth, minFeatureSize, searchScaleFactor, minNeighbors);

            if (objects.Length > 0) return objects;
            else return null;
        }
    }
}