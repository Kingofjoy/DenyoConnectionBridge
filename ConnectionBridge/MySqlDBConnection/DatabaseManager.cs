
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
        private MySqlConnection connection;

        //Constructor
        public DatabaseManager()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {

            string connectionString=string.Empty;
            //connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
            connection = new MySqlConnection(connectionString);
        }


        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
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
                connection.Close();
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
            try
            {
                //open connection
                if (this.OpenConnection() == true)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    iAffectedRows = cmd.ExecuteNonQuery();

                }
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                if(this.connection !=null && this.connection.State != System.Data.ConnectionState.Closed)
                    this.CloseConnection();
            }
            return iAffectedRows;
        }
        
        //Select Statements
        public DataSet ExecuteDataSet(string query)
        {
            DataSet dsData = new DataSet();
            try
            {
                if (this.OpenConnection())
                {
                    MySqlDataAdapter sqlDA = new MySqlDataAdapter(query, connection);
                    MySqlCommandBuilder cmdbd = new MySqlCommandBuilder(sqlDA);
                    sqlDA.Fill(dsData);
                    
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (this.connection != null && this.connection.State != System.Data.ConnectionState.Closed)
                    this.CloseConnection();
            }
            return dsData;
        }
                
        //Count statements
        public int ExecuteCount(string query)
        {
            int Count = -1;

            try
            {
                //Open Connection
                if (this.OpenConnection() == true)
                {
                    //Create Mysql Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //ExecuteScalar will return one value
                    Count = int.Parse(cmd.ExecuteScalar().ToString());

                    //close Connection
                    this.CloseConnection();

                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if (this.connection != null && this.connection.State != System.Data.ConnectionState.Closed)
                    this.CloseConnection();
            }
            return Count;
        }

    }
}
