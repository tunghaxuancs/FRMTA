using System.Drawing;

namespace FR.Client.Scripts
{
    public class Record
    {
        private int _ID;
        private Bitmap _DetectImg;
        private Bitmap _PredictImg;
        private Bitmap _PreProcessImg;

        public Bitmap PreProcessImg
        {
            get { return _PreProcessImg; }
            set { _PreProcessImg = value; }
        }
        private string _Info;

        public Record(int id, Bitmap detect, Bitmap predict, Bitmap preProcess, string info)
        {
            _ID = id;
            _DetectImg = detect;
            _PredictImg = predict;
            _PreProcessImg = preProcess;
            _Info = info;
        }

        public string Info
        {
            get { return _Info; }
            set { _Info = value; }
        }

        public Bitmap PredictImg
        {
            get { return _PredictImg; }
            set { _PredictImg = value; }
        }

        public Bitmap DetectImg
        {
            get { return _DetectImg; }
            set { _DetectImg = value; }
        }

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
    }
}