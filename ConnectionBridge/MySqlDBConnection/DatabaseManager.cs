
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;

namespace Denyo.ConnectionBridge.MySqlDBConnection
{
    public class DatabaseManager
    {
        private MySqlConnection Testconnection;

        string ConnectionString;

        //Constructor
        public DatabaseManager()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {

            //connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            Testconnection = new MySqlConnection(ConnectionString);
            Testconnection.Open();
        }


        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                Testconnection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        throw new Exception("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        throw new Exception("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                Testconnection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                throw new Exception(ex.Message);
                return false;
            }
        }

        //Insert / Update / Delete statements
        public int ExecuteNonQuery(string query)
        {
            int iAffectedRows = -1;
            MySqlConnection connection=null;
            try
            {
                connection = new MySqlConnection(ConnectionString);
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    iAffectedRows = cmd.ExecuteNonQuery();

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("DBX "+ ex.Message);
                throw;
            }
            finally
            {
                if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
            return iAffectedRows;
        }
        
        //Select Statements
        public DataSet ExecuteDataSet(string query)
        {
            DataSet dsData = new DataSet();
            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    MySqlDataAdapter sqlDA = new MySqlDataAdapter(query, connection);
                    MySqlCommandBuilder cmdbd = new MySqlCommandBuilder(sqlDA);
                    sqlDA.Fill(dsData);
                    
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
            return dsData;
        }
                
        //Count statements
        public int ExecuteCount(string query)
        {
            int Count = -1;

            MySqlConnection connection = new MySqlConnection(ConnectionString);
            try
            {
                connection.Open();
                //Open Connection
                if (connection.State == ConnectionState.Open)
                {
                    //Create Mysql Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //ExecuteScalar will return one value
                    Count = int.Parse(cmd.ExecuteScalar().ToString());
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            }
            return Count;
        }

        public int UpdateSetPoints(string DeviceID, string Type, string Label, string Value, string Hex_Value, DateTime ReceivedTime, string Comment)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("sp_setpointstorage", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("@pUnit_Name", DeviceID));
                        cmd.Parameters.Add(new MySqlParameter("@pType", Type));
                        cmd.Parameters.Add(new MySqlParameter("@pLabel", Label));
                        cmd.Parameters.Add(new MySqlParameter("@pValue", Value));
                        cmd.Parameters.Add(new MySqlParameter("@pHexa_Value", Hex_Value));
                        cmd.Parameters.Add(new MySqlParameter("@pReceived_Time", ReceivedTime));
                        cmd.Parameters.Add(new MySqlParameter("@pComment", Comment));
                        cmd.Parameters.Add(new MySqlParameter("@pRows_Affected", MySqlDbType.Int32)); // .Direction = ParameterDirection.Output);
                        cmd.Parameters["@pRows_Affected"].Direction = ParameterDirection.Output;
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        return (int)cmd.Parameters["@pRows_Affected"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateSetPoints err:" + ex.Message);
                return -1;
            }
        }


        public int UpdateAlarms(string DeviceID, string Alarm, DateTime ReceivedTime)
        {
            try
            {
                //Console.WriteLine("sp_UpdateAlarms");
                using (MySqlConnection con = new MySqlConnection(ConnectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("sp_UpdateAlarms", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("@pUnit_Name", DeviceID));
                        cmd.Parameters.Add(new MySqlParameter("@pAlarm_Name", Alarm));
                        cmd.Parameters.Add(new MySqlParameter("@pReceived_Time", ReceivedTime));
                        cmd.Parameters.Add(new MySqlParameter("@pRows_Affected", MySqlDbType.Int32));
                        cmd.Parameters["@pRows_Affected"].Direction = ParameterDirection.Output;
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        return (int)cmd.Parameters["@pRows_Affected"].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateAlarms err:" + ex.Message);
                return -1;
            }
        }
    }
}
