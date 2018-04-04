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
        public bool UpdateMonitoringStatus(string DeviceID, string Input, string Output, string OutputHexa, DateTime UpdatedOn)
        {
            string query = "";
            try
            {
                query = @"update unit_currentstatus set actual_hexacode=" + (string.IsNullOrEmpty(OutputHexa) ? "null" : "'" + OutputHexa + "'") +
                                                            ",  timestamp='" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "',  value=" + (string.IsNullOrEmpty(Output) ? "null" : "'" + Output + "'") +
                                                            " where generatorid='" + DeviceID + "' and code='" + Input + "'";

                if (!(datamanger.ExecuteNonQuery(query) > 0))
                {
                    query = @"INSERT INTO `unit_currentstatus` (`generatorid`, `code`, `value`, `actual_hexacode`, `timestamp`) 
                            VALUES ('" + DeviceID + "', '" + Input + "','"+ Output+ "', '" + OutputHexa + "', '" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "');";
                    if (datamanger.ExecuteNonQuery(query) > 0)
                        return true;
                }
                else return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                //throw;
            }
            return false;
        }

        public bool DeleteAlarms(string DeviceID)
        {
            string query = "";
            try
            {
                query = @"delete FROM `alarms` where alarm_unit_id IN (SELECT `unit_id` FROM `units` WHERE controllerid = '" + DeviceID + "')";

                if ((datamanger.ExecuteNonQuery(query) > 0))
                {
                    //query = @"INSERT INTO `unit_currentstatus` (`generatorid`, `code`, `value`, `actual_hexacode`, `timestamp`) 
                    //        VALUES ('" + DeviceID + "', '" + Input + "', NULL, '" + Output + "', '" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "');";
                    //if (datamanger.ExecuteNonQuery(query) > 0)
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("DeleteAlarms err:" + ex.Message);
                Console.WriteLine(query);
                return false;
            }
        }

        public bool UpdateAlarms(string DeviceID, string AlarmValue, string AlarmHex, DateTime ReceivedDateTime)
        {
            bool status=false;
            try
            {
                if (datamanger.UpdateAlarms(DeviceID, AlarmValue, AlarmHex, ReceivedDateTime) > 0)
                    status = true;
              

            }catch(Exception ex)
            {
                status = false;
                Console.WriteLine("UpdateAlarms err:" + ex.Message);
            }
            return status;
        }
        public bool UpdateAlarms(string DeviceID, List<string> Alarms)
        {
            string query = "";
            bool status = true;
            try
            {
                DeleteAlarms(DeviceID);
                foreach (string alarm in Alarms)
                {
                    query = @"INSERT INTO `alarms`(`alarm_name`, `alarm_unit_id`, `alarm_assigned_date`, `alarm_remark`) 
                            VALUES ('" + alarm + "', (SELECT `unit_id` FROM `units` WHERE controllerid = '" + DeviceID + "'), CURRENT_TIMESTAMP ,'');";
                    if (!(datamanger.ExecuteNonQuery(query) > 0))
                        status = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateAlarms err:" + ex.Message);
                Console.WriteLine(query);
                return false;
            }
            return status;
        }

        public bool UpdateSetPoints(string DeviceID, string Type, string Label, string Value, string Hex_Value, DateTime ReceivedTime, string Comment)
        {
            try
            {
                if (datamanger.UpdateSetPoints(DeviceID, Type, Label, Value, Hex_Value, ReceivedTime, Comment) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateSetPoints err:" + ex.Message);
                return false;
            }
        }

        public bool UpdateGPSStatus(string DeviceID, string Input, string Output, string OutputHexa, DateTime UpdatedOn)
        {
            string query = "";
            try
            {
                query = @"update gps_currentstatus set actual_hexacode=" + (string.IsNullOrEmpty(OutputHexa) ? "null" : "'" + OutputHexa + "'") +
                                                            ",  timestamp='" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "',  value=" + (string.IsNullOrEmpty(Output) ? "null" : "'" + Output + "'") +
                                                            " where generatorid='" + DeviceID + "' and code='" + Input + "'";

                if (!(datamanger.ExecuteNonQuery(query) > 0))
                {
                    query = @"INSERT INTO `gps_currentstatus` (`generatorid`, `code`, `value`, `actual_hexacode`, `timestamp`) 
                            VALUES ('" + DeviceID + "', '" + Input + "', '" + Output + "', '" + OutputHexa + "', '" + UpdatedOn.ToString("yyyy-MM-dd H:mm:ss") + "');";
                    if (datamanger.ExecuteNonQuery(query) > 0)
                        return true;
                }
                else return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                //throw;
            }
            return false;
        }
    }
}
