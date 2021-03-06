namespace ZMQ.Extensions
{
	using System;
	using ZMQ;

	public class ZContextAccessor : IDisposable
	{
		private const int IoThreads = 2;

		private readonly Lazy<Context> context = new Lazy<Context>(() => new Context(IoThreads), true);

		private Func<SocketType, ZSocket> socketFactory = type => new ZSocket(type);

		public Context Current
		{
			get { return context.Value; }
		}

		public Func<SocketType, ZSocket> SocketFactory
		{
			get { return socketFactory; }
		}

		public void Dispose()
		{
			//if (context.IsValueCreated)
			//	context.Value.Dispose();
		}

		public static ZContextAccessor New(Func<SocketType, ZSocket> socketFactory) 
		{
			return new ZContextAccessor { socketFactory = socketFactory };
		}
	}
}