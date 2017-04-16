using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FR.Data
{
    public class FaceData
    {
        private int _ID;
        private int _CustomerID;
        private string _FaceFolder;
        private string _FaceImage;

        public string FaceImage
        {
            get { return _FaceImage; }
            set { _FaceImage = value; }
        }

        public string FaceFolder
        {
            get { return _FaceFolder; }
            set { _FaceFolder = value; }
        }

        public int CustomerID
        {
            get { return _CustomerID; }
            set { _CustomerID = value; }
        }

        public int ID
        {
            get { return _ID; }
        }
    }
}
