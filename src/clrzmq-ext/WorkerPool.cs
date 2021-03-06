﻿/*
    Copyright (c) 2011 Michael Compton <michael.compton@littleedge.co.uk>

    This file is part of clrzmq2.

    clrzmq2 is free software; you can redistribute it and/or modify it under
    the terms of the Lesser GNU General Public License as published by
    the Free Software Foundation; either version 3 of the License, or
    (at your option) any later version.

    clrzmq2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    Lesser GNU General Public License for more details.

    You should have received a copy of the Lesser GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.

*/

using System.Threading;

namespace ZMQ.ZMQDevice {
	using System;
	using System.Diagnostics;

	/// <summary>
    /// Worker pool device
    /// </summary>
    public class WorkerPool : Queue {
        private Thread[] workerThreads;


        public WorkerPool(string inSktAddr, string outSktAddr, ThreadStart worker, short workerCount)
            : base(inSktAddr, outSktAddr) {
            Start();
            CreateWorkerThreads(worker, workerCount, inSktAddr);
        }

        private void CreateWorkerThreads(ThreadStart worker, short workerCount, string name) {
            workerThreads = new Thread[workerCount];
            for (short count = 0; count < workerCount; count++) {
                workerThreads[count] = new Thread(worker) {Name =  string.Format("Worker [{0}] for [{1}]", count, name), IsBackground = true};
                workerThreads[count].Start();
            }
        }

	    protected override void Dispose(bool disposing)
	    {
			base.Dispose(disposing);

			if (workerThreads != null)
				foreach (var workerThread in workerThreads)
				{
					try
					{
						workerThread.Abort();
					}
					catch (ThreadAbortException)
					{
					}
				}
	    }
    }
}
