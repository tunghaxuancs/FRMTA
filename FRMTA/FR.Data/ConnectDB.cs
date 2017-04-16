using System;
using System.Data.SqlClient;
using System.Data;

namespace FR.Data
{
    public class ConnectDB
    {
        public static SqlConnection connect;
        public static void OpenToConnect()
        {
            if (ConnectDB.connect == null)
            {
                try
                {
                    ConnectDB.connect = new SqlConnection(@"Server=.\SQLEXPRESS;Initial Catalog=FRMTA;Integrated Security=SSPI;MultipleActiveResultSets=True");
                }
                catch
                {
                }
            }

            if (ConnectDB.connect.State != ConnectionState.Open)
                ConnectDB.connect.Open();
        }
        public static void CloseAfterConnect()
        {
            if (ConnectDB.connect != null)
            {
                if (ConnectDB.connect.State == ConnectionState.Open)
                {
                    ConnectDB.connect.Close();
                }
            }
        }
        public int ExecuteScalar(string strSQL)
        {
            try
            {
                OpenToConnect();
                SqlCommand sqlcmd = new SqlCommand(strSQL, connect);
                int id = (int) sqlcmd.ExecuteScalar();
                CloseAfterConnect();
                return id;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        public void ExecuteNonQuery(string strSQL)
        {
            try
            {
                OpenToConnect();
                SqlCommand sqlcmd = new SqlCommand(strSQL, connect);
                sqlcmd.ExecuteNonQuery();
                CloseAfterConnect();
            }
            catch
            {
            }
        }
        public DataTable GetDataTable(string strSQL)
        {
            try
            {
                OpenToConnect();
                DataTable dt = new DataTable();
                SqlDataAdapter sqlda = new SqlDataAdapter(strSQL, connect);
                sqlda.Fill(dt);
                CloseAfterConnect();
                return dt;
            }
            catch
            {
                return null;
            }
        }
        public string GetValue(string strSQL, int k)
        {
            string temp = null;
            OpenToConnect();
            SqlCommand sqlcmd = new SqlCommand(strSQL, connect);
            SqlDataReader sqldr = sqlcmd.ExecuteReader();
            while (sqldr.Read())
            {
                temp = sqldr[k].ToString();
            }
            CloseAfterConnect();
            return temp;
        }
        public SqlDataAdapter GetCmd(string strSQL)
        {
            SqlDataAdapter sql = new SqlDataAdapter(strSQL, connect);
            SqlCommandBuilder cmd = new SqlCommandBuilder(sql);

            sql.InsertCommand = cmd.GetInsertCommand();
            sql.UpdateCommand = cmd.GetUpdateCommand();
            sql.DeleteCommand = cmd.GetDeleteCommand();

            return sql;
        }
    }
}