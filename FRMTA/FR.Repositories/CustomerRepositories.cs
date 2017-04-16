using FR.Data;
using System.Data;

namespace FR.Repositories
{
    public class CustomerRepositories
    {
        private ConnectDB connectDB = new ConnectDB();
        public DataTable SelectData(string where)
        {
            string query = "select * from Customer Where Status='True' " + (string.IsNullOrEmpty(where) ? string.Empty : (" and " + where));
            return connectDB.GetDataTable(query);
        }
        public int InsertData(Customer customer)
        {
            string query = @"INSERT INTO Customer 
                         (FullName, Birthday, Address, Mobile, Career, Status) OUTPUT INSERTED.ID VALUES 
                         (N'" + customer.FullName + "',N'" + customer.BirthDay.ToShortDateString() + "',N'" + customer.Address + "',N'" +
                             customer.Mobile + "',N'" + customer.Career + "','True')";
            return connectDB.ExecuteScalar(query);
        }
        public void UpdateData(Customer customer)
        {
            string query = @"UPDATE Customer SET
                            FullName =N'" + customer.FullName + "',Birthday =N'" + customer.BirthDay.ToShortDateString() +
                            "',Address =N'" + customer.Address + "',Mobile =N'" + customer.Mobile + "',Career =N'" +
                            customer.Career + "',Status ='" + customer.Status + "' WHERE ID=N'" + customer.ID + "'";
            connectDB.ExecuteNonQuery(query);
        }
        public void DeleteData(int id)
        {
            string query = @"UPDATE Customer SET Status = False WHERE ID=N'" + id.ToString() + "'";
            connectDB.ExecuteNonQuery(query);
        }
    }
}