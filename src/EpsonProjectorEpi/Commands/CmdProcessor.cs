﻿using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using PepperDash.Core;

namespace EpsonProjectorEpi.Commands
{
    public class CommandProcessor : IDisposable
    {
        private readonly CrestronQueue<IEpsonCmd> _cmdQueue;
        private readonly Thread _worker;
        private readonly CEvent _wh = new CEvent();
        private readonly CCriticalSection _lock = new CCriticalSection();

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
                        Debug.Console(2, coms, "Sending a string {0}", cmd.CmdString);
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
            try
            {
                _lock.Enter();
                _cmdQueue.Enqueue(cmd);
                _wh.Set();
            }
            finally
            {
                _lock.Leave();
            }
        }

        public void EnqueueCmd(params IEpsonCmd[] cmds)
        {
            try
            {
                _lock.Enter();
                foreach (var cmd in cmds)
                    _cmdQueue.Enqueue(cmd);

                _wh.Set();
            }
            finally
            {
                _lock.Leave();
            }
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
                _cmdQueue.Enqueue(null);
                _wh.Set();
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