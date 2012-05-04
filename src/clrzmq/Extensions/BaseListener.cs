﻿namespace ZMQ.Extensions
{
	using System;
	using System.Threading;
	using Castle.Core.Logging;
	using ZMQ;

	public abstract class BaseListener : IDisposable
	{
		private Thread thread;
		private bool disposed;

		protected BaseListener(ZContextAccessor zContextAccessor)
		{
			ContextAccessor = zContextAccessor;
			
			Logger = NullLogger.Instance;
		}

		protected ZContextAccessor ContextAccessor { get; set; }

		public ILogger Logger { get; set; }

		protected abstract ZConfig GetConfig();

		protected abstract byte[] GetReplyFor(byte[] request, ZSocket socket);

		public void Start()
		{
			Logger.Debug("Starting " + GetType().Name);

			try
			{
				thread = new Thread(() =>
				                    	{
				                    		try
				                    		{
												var config = GetConfig();

				                    			var socket = ContextAccessor.SocketFactory(SocketType.REP);

				                    			socket.Bind(config.Transport, config.Ip, config.Port);

												Logger.InfoFormat("Binding {0} on {1}:{2}", GetType().Name, config.Ip, config.Port);

				                    			while (true)
				                    			{
				                    				var bytes = socket.Recv(int.MaxValue);

				                    				byte[] reply = null;

				                    				try
				                    				{
				                    					reply = bytes == null ? new byte[0] : GetReplyFor(bytes, socket);
				                    				}
				                    				catch (System.Exception e)
				                    				{
				                    					Logger.Error("Error getting reply.", e);
				                    				}
													finally
				                    				{
				                    					socket.Send(reply ?? new byte[0]);
				                    				}
				                    			}
				                    		}
				                    		catch (System.Exception e)
				                    		{
				                    			Logger.Fatal("Error on " + GetType().Name + " background thread", e);
				                    		}

				                    	})
				         	{
				         		IsBackground = true,
				         		Name = "Worker thread for " + GetType().Name
				         	};

				thread.Start();
			}
			catch (System.Exception e)
			{
				Logger.Error("Error starting " + GetType().Name, e);
			}
		}

		public void Stop()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (disposed) return;

			disposed = true;

			Logger.Info("Disposing " + GetType().Name);

			if (thread != null)
			{
                thread.Abort(); // cr: same comment
			    thread = null;
			}
		}
	}
}