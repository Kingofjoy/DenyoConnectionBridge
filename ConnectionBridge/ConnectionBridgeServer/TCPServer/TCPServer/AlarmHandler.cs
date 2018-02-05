﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Denyo.ConnectionBridge.Server.TCPServer
{
    class AlarmHandler
    {
        ConcurrentDictionary<string, int> AlarmMaster = new ConcurrentDictionary<string, int>();
        ConcurrentDictionary<string, List<string>> Alarams = new ConcurrentDictionary<string, List<string>>();
        DBInteractions dbInteraction;

        public AlarmHandler()
        {
            try
            {
                dbInteraction = new DBInteractions();
            }
            catch(Exception ex)
            {

            }
        }
        
        // Update count
        public bool AlarmMasterUpdate(string senderId, int output)
        {
            try
            {
                int noOfAlarm;
                if(AlarmMaster.TryGetValue(senderId, out noOfAlarm))
                {
                    if(output != noOfAlarm)
                    AlarmMaster[senderId] = output;
                    if(output == 0)
                    {
                        List<string> dummylist = new List<string>();
                        Alarams.TryRemove(senderId, out dummylist);
                        dbInteraction.DeleteAlarms(senderId);
                    }
                }
                else if (output > 0)
                {
                    AlarmMaster.TryAdd(senderId, output);
                }
            }
            catch(Exception ex)
            {
                //Logger.Log("AlarmMasterUpdate Error:", ex);
                return false;
            }
            return true;
        }

        public void AlarmUpdate(string senderId, string output)
        {
            try
            {
                int noOfAlarm;
                if (AlarmMaster.TryGetValue(senderId, out noOfAlarm) && noOfAlarm > 0)
                {
                    if (!Alarams.ContainsKey(senderId))
                        Alarams.TryAdd(senderId, new List<string>());
                    if (!string.IsNullOrEmpty(output) && !Alarams[senderId].Contains(output))
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(output, @"^[a-zA-Z0-9 ]+$"))
                        {
                            Alarams[senderId].Add(output);
                            dbInteraction.UpdateAlarms(senderId, Alarams[senderId]);
                        }
                        else
                        Console.WriteLine("Alarm Update Skip, Junk value"+senderId+ " : "+output);
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Log("AlarmUpdate Error:", ex);
            }
        }

        // Upon receiving A
        // If a value is string valid
        // if dic list of genx not alredy present, then add else do nothing

        // if alaram master is not empty
        // delete alrams in db for genx
        // insert dic list items into db
        // if received alram count is 0, then clear list update db
    }
}
