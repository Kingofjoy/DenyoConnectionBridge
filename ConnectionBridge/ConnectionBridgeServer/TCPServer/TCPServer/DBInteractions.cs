using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data;
using Denyo.ConnectionBridge.MySqlDBConnection;

namespace Denyo.ConnectionBridge.Server.TCPServer
{
    class DBInteractions
    {

        DatabaseManager datamanger;
        public DBInteractions()
        {
            datamanger = new DatabaseManager();
        }
        public bool UpdateMonitoringStatus(string DeviceID,string Input,string Output,string OutputHexa,DateTime UpdatedOn)
        {
            string query = "";
            try
            {
                query = @"update unit_currentstatus set actual_hexacode=" + (string.IsNullOrEmpty(OutputHexa)?"null":"'"+OutputHexa+"'") +
                                                            ",  timestamp='" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "',  value=" + (string.IsNullOrEmpty(Output) ? "null" : "'" + Output + "'") +
                                                            " where generatorid='" + DeviceID + "' and code='" + Input + "'";

                if (!(datamanger.ExecuteNonQuery(query) > 0))
                {
                    query = @"INSERT INTO `unit_currentstatus` (`generatorid`, `code`, `value`, `actual_hexacode`, `timestamp`) 
                            VALUES ('" + DeviceID + "', '" + Input + "', NULL, '" + Output + "', '" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "');";
                    if (datamanger.ExecuteNonQuery(query) > 0)
                        return true;
                }
                else return true;

            }
            catch(Exception ex)
            {
                Console.WriteLine(query);
                throw;
            }
            return false;
        }
    }
}
