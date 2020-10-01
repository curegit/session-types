using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Session
{
	public abstract class Session
	{
		private bool used;

		private Task? lastTask;

		private protected ICommunicator communicator;

		/*
		internal ICommunicator Communicator
		{
			get
			{
				return communicator;
			}
			set
			{
				communicator = value ?? throw new ArgumentNullException(nameof(Communicator));
			}
		}*/

		private protected Session(Session session)
		{
			communicator = session.communicator;
			lastTask = session.lastTask;
		}

		private protected Session(ICommunicator communicator)
		{
			this.communicator = communicator;
		}

		internal void Cancel()
		{
			communicator.Cancel();
		}

		internal void WaitForLastTask()
		{
			lastTask?.Wait();
			lastTask = null;
		}

		internal async Task AwaitLastTask()
		{
			if (lastTask != null)
			{
				await lastTask;
				lastTask = null;
			}
		}

		protected Task ContinueAsync(Func<Task> continuation)
		{
			async Task AppendContinuation()
			{
				if (lastTask != null)
				{
					await lastTask;
				}
				await continuation();
			}
			var task = AppendContinuation();
			lastTask = task;
			return task;
		}

		protected Task<T> ContinueAsync<T>(Func<Task<T>> continuation)
		{
			async Task<T> AppendContinuation()
			{
				if (lastTask != null)
				{
					await lastTask;
				}
				return await continuation();
			}
			var task = AppendContinuation();
			lastTask = task;
			return task;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string ToString()
		{
			return base.ToString();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		internal S ToNextSession<S>() where S : Session, new()
		{
			return new S
			{
				communicator = communicator,
				lastTask = lastTask
			};
			//return s;
		}

		internal S Duplicate<S>() where S : Session, new()
		{
			return ToNextSession<S>();
		}

		internal static S Create<S>(ICommunicator communicator) where S : Session, new()
		{
			var s = new S
			{
				communicator = communicator
			};
			return s;
		}

		internal void Send()
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Send();
		}

		internal Task SendAsync()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.SendAsync());
		}

		internal void Send<T>(T value)
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Send(value);
		}

		internal Task SendAsync<T>(T value)
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.SendAsync(value));
		}

		internal void Receive()
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Receive();
		}

		internal Task ReceiveAsync()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.ReceiveAsync());
		}

		internal T Receive<T>()
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.Receive<T>();
		}

		internal Task<T> ReceiveAsync<T>()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.ReceiveAsync<T>());
		}

		internal Z ThrowNewChannel<Z>() where Z : Session, new()
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.ThrowNewChannel<Z>();
		}

		internal Task<Z> ThrowNewChannelAsync<Z>() where Z : Session, new()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.ThrowNewChannelAsync<Z>());
		}

		internal Z CatchNewChannel<Z>() where Z : Session, new()
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.CatchNewChannel<Z>();
		}

		internal Task<Z> CatchNewChannelAsync<Z>() where Z : Session, new()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.CatchNewChannelAsync<Z>());
		}

		internal void Select(Selection selection)
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Select(selection);
		}

		internal Task SelectAsync(Selection selection)
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.SelectAsync(selection));
		}

		internal Selection Follow()
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.Follow();
		}

		internal Task<Selection> FollowAsync()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.FollowAsync());
		}

		internal void CallSimply()
		{
			TrySpendLinearity();
		}

		internal void Close()
		{
			TrySpendLinearity();
			communicator.Close();
		}

		internal async Task CloseAsync()
		{
			TrySpendLinearity();
			await AwaitLastTask();
			communicator.Close();
		}

		private void TrySpendLinearity()
		{
			if (used)
			{
				throw new LinearityViolationException();
			}
			else
			{
				used = true;
			}
		}
	}

	/*
	public sealed class Session<S> : Session where S : Session where E : SessionStack where P : ProtocolType
	{
		

		private Session(Session session) : base(session) { }

		internal Session(ICommunicator communicator) : base(communicator) { }

		internal Session<S, E, P> Duplicate()
		{
			return new Session<S, E, P>(this);
		}

		internal Session<Z, E, P> ToNextSession<Z>() where Z : SessionType
		{
			return new Session<Z, E, P>(this);
		}

		internal Session<Z, H, P> ToNextSession<Z, H>() where Z : SessionType where H : SessionStack
		{
			return new Session<Z, H, P>(this);
		}

		internal void Send()
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Send();
		}

		internal Task SendAsync()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.SendAsync());
		}

		internal void Send<T>(T value)
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Send(value);
		}

		internal Task SendAsync<T>(T value)
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.SendAsync(value));
		}

		internal void Receive()
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Receive();
		}

		internal Task ReceiveAsync()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.ReceiveAsync());
		}

		internal T Receive<T>()
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.Receive<T>();
		}

		internal Task<T> ReceiveAsync<T>()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.ReceiveAsync<T>());
		}

		internal Session<Z, Empty, Q> ThrowNewChannel<Z, Q>() where Z : SessionType where Q : ProtocolType
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.ThrowNewChannel<Z, Q>();
		}

		internal Task<Session<Z, Empty, Q>> ThrowNewChannelAsync<Z, Q>() where Z : SessionType where Q : ProtocolType
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.ThrowNewChannelAsync<Z, Q>());
		}

		internal Session<Z, Empty, Q> CatchNewChannel<Z, Q>() where Z : SessionType where Q : ProtocolType
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.CatchNewChannel<Z, Q>();
		}

		internal Task<Session<Z, Empty, Q>> CatchNewChannelAsync<Z, Q>() where Z : SessionType where Q : ProtocolType
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.CatchNewChannelAsync<Z, Q>());
		}

		internal void Select(Selection selection)
		{
			TrySpendLinearity();
			WaitForLastTask();
			communicator.Select(selection);
		}

		internal Task SelectAsync(Selection selection)
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.SelectAsync(selection));
		}

		internal Selection Follow()
		{
			TrySpendLinearity();
			WaitForLastTask();
			return communicator.Follow();
		}

		internal Task<Selection> FollowAsync()
		{
			TrySpendLinearity();
			return ContinueAsync(() => communicator.FollowAsync());
		}

		internal void CallSimply()
		{
			TrySpendLinearity();
		}

		internal void Close()
		{
			TrySpendLinearity();
			communicator.Close();
		}

		internal async Task CloseAsync()
		{
			TrySpendLinearity();
			await AwaitLastTask();
			communicator.Close();
		}

		private void TrySpendLinearity()
		{
			if (used)
			{
				throw new LinearityViolationException();
			}
			else
			{
				used = true;
			}
		}
		
	}*/
}
