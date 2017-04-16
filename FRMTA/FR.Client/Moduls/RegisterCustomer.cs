using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using FR.Data;
using FR.Repositories;

namespace FR.Client.Moduls
{
    public partial class RegisterCustomer : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private CustomerRepositories repositories = new CustomerRepositories();
        public Customer customer;
        public RegisterCustomer()
        {
            InitializeComponent();
        }
        private bool CheckInput()
        {
            foreach (Control c in layoutControl1.Controls)
            {
                if (c.GetType() == typeof(DevExpress.XtraEditors.TextEdit) ||
                    c.GetType() == typeof(DevExpress.XtraEditors.DateEdit))
                {
                    if (c.Tag != null && string.IsNullOrEmpty((c as DevExpress.XtraEditors.TextEdit).Text))
                    {
                        MessageBox.Show("Please input field " + c.Name.Remove(0, 3));
                        return false;
                    }
                }
            }
            return true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (CheckInput())
            {
                customer = new Customer()
                {
                    FullName = txtFullname.Text,
                    Address = txtAddress.Text,
                    BirthDay = txtBirthday.DateTime.Date,
                    Career = txtCareer.Text,
                    Mobile = txtMobile.Text
                };

                customer.ID = repositories.InsertData(customer);

                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}