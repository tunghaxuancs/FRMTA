using FR.Data;
using System.Data;

namespace FR.Repositories
{
    public class FaceDataRepositories
    {
        private ConnectDB connectDB = new ConnectDB();

        public DataTable SelectData(string where)
        {
            string query = "select * from FaceData " + (string.IsNullOrEmpty(where) ? string.Empty : (" where " + where));
            return connectDB.GetDataTable(query);
        }

        public int InsertData(FaceData faceData)
        {
            string query = @"INSERT INTO FaceData (CustomerID, FaceFolder, FaceImage) OUTPUT INSERTED.Id VALUES (N'" + faceData.CustomerID + "',N'" +
                faceData.FaceFolder + "',N'" + faceData.FaceImage + "')";
            return connectDB.ExecuteScalar(query);
        }

        public void UpdateData(FaceData faceData)
        {
            string query = @"UPDATE FaceData SET CustomerID =N'" + faceData.CustomerID + "', FaceFolder =N'" + faceData.FaceFolder +
                "', FaceImage =N'" + faceData.FaceImage + "'";
            connectDB.ExecuteNonQuery(query);
        }

        public void DeleteData(int id)
        {
            string query = @"DELETE FROM FaceData WHERE ID=N'" + id.ToString() + "'";
            connectDB.ExecuteNonQuery(query);
        }
    }
}