using System;
namespace FR.Data
{
    public class Customer
    {
        private int _ID;
        private string _FullName;
        private string _Address;
        private DateTime _BirthDay;
        private string _Mobile;
        private string _Career;
        private bool _Status;

        public bool Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        public string Career
        {
            get { return _Career; }
            set { _Career = value; }
        }

        public string Mobile
        {
            get { return _Mobile; }
            set { _Mobile = value; }
        }

        public DateTime BirthDay
        {
            get { return _BirthDay; }
            set { _BirthDay = value; }
        }
        public string Address
        {
            get { return _Address; }
            set { _Address = value; }
        }
        public string FullName
        {
            get { return _FullName; }
            set { _FullName = value; }
        }

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
    }
}