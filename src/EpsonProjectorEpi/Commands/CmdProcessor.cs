using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using EpsonProjectorEpi.Enums;
using EpsonProjectorEpi.Commands;
using EpsonProjectorEpi.Queries;
using EpsonProjectorEpi.States.Power;

namespace EpsonProjectorEpi.Commands
{
    public class CmdProcessor
    {
        private readonly CrestronQueue<IEpsonCmd> _cmdQueue;
        private readonly Thread _worker;
        private readonly CEvent _wh = new CEvent();

        public CmdProcessor(IBasicCommunication coms)
        {
            _cmdQueue = new CrestronQueue<IEpsonCmd>();
            _worker = new Thread(ProcessCmd, coms, Thread.eThreadStartOptions.Running);

            CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
                {
                    switch (programEvent)
                    {
                        case eProgramStatusEventType.Stopping:
                            {
                                _cmdQueue.Enqueue(null);
                                _worker.Join();
                                _wh.Close();
                                break;
                            }
                    };
                };
        }

        object ProcessCmd(object obj)
        {
            var coms = obj as IBasicCommunication;

            while (true)
            {
                IEpsonCmd cmd = null;

                if (_cmdQueue.Count > 0)
                {
                    cmd = _cmdQueue.Dequeue();
                    if
                        (cmd == null) break;
                }
                if (cmd != null)
                {
                    try
                    {
                        Debug.Console(0, coms, "Sending a string {0}", cmd.CmdString);
                        new CmdHandler(coms, cmd).Handle();
                        Thread.Sleep(50);
                    }
                    catch (Exception ex)
                    {
                        Debug.ConsoleWithLog(0, coms, "Caught an exception in the CmdProcessor {0}\r{1}\r{2}", ex.Message, ex.InnerException, ex.StackTrace);
                    }
                }
                else _wh.Wait();
            }

            return null;
        }

        public void EnqueueCmd(IEpsonCmd cmd)
        {
            _cmdQueue.Enqueue(cmd);
            _wh.Set();
        }
    }
}