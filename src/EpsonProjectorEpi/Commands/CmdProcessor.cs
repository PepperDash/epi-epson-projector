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

namespace EpsonProjectorEpi.Commands
{
    public class CommandProcessor : IDisposable
    {
        private readonly CrestronQueue<IEpsonCmd> _cmdQueue;
        private readonly Thread _worker;
        private readonly CEvent _wh = new CEvent();

        public bool Disposed { get; private set; }

        public CommandProcessor(IBasicCommunication coms)
        {
            _cmdQueue = new CrestronQueue<IEpsonCmd>();
            _worker = new Thread(ProcessCmd, coms, Thread.eThreadStartOptions.Running);

            CrestronEnvironment.ProgramStatusEventHandler += programEvent =>
                {
                    if (programEvent != eProgramStatusEventType.Stopping)
                        return;

                    Debug.Console(1, coms, "Shutting down the coms processor...");
                    Dispose();
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
                    if (cmd == null) 
                        break;
                }
                if (cmd != null)
                {
                    try
                    {
                        Debug.Console(1, coms, "Sending a string {0}", cmd.CmdString);
                        new CommandHandler(coms, cmd).Handle();
                        Thread.Sleep(500);
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

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                EnqueueCmd(null);
                _worker.Join();
                _wh.Close();
            }

            Disposed = true;
        }

        ~CommandProcessor()
        {
            Dispose(false);
        }

        #endregion
    }
}