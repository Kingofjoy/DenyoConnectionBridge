using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenyoConnectionBridgeService
{
    public class Executor
    {
        public delegate void RunWhenFaild();

        public void Execute(Action functionToBeExecuted, Action<Exception> callOnFailure)
        {
            RunWhenFaild.Combine(callOnFailure);
            //RunWhenFaild += callOnFailure;

            try
            {
                functionToBeExecuted();
                //RunWhenFaild -= callOnFailure;
                
            }
            catch (Exception e)
            {
                callOnFailure(e);
            }
        }

       
    }
}
