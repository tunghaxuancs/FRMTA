using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FR.Client.Moduls
{
    public partial class GetTextField : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private string _textField;

        public string TextField
        {
            get { return _textField; }
            set { _textField = PreprocessText(value); }
        }
        public GetTextField()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTextField.Text)) { MessageBox.Show("Please input text field!"); return; }
            _textField = txtTextField.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private string PreprocessText(string value)
        {
            value = value.Replace(' ', '_');
            value = value.Replace('.', '_');
            return value;
        }
    }
}
