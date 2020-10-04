using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Session
{
	public static class SessionUtility
	{
		public static S Let<S, T>(this S session, out T variable, T value) where S : Session, new()
		{
			if (session is null) throw new ArgumentNullException(nameof(session));
			session.CallSimply();
			variable = value;
			return session.Duplicate<S>();
		}

		public static S Wait<S>(this S session) where S : Session, new()
		{
			if (session is null) throw new ArgumentNullException(nameof(session));
			session.CallSimply();
			session.WaitForLastTask();
			return session.Duplicate<S>();
		}

		public static async Task<S> Sync<S>(this S session) where S : Session, new()
		{
			if (session is null) throw new ArgumentNullException(nameof(session));
			session.CallSimply();
			await session.AwaitLastTask();
			return session.Duplicate<S>();
		}

		public static void ForEach<S>(this IEnumerable<S> sessions, Action<S> action) where S : Session
		{
			foreach (var session in sessions)
			{
				action(session);
			}
		}

		public static IEnumerable<T> Map<S, T>(this IEnumerable<S> sessions, Func<S, T> func) where S : Session
		{
			if (sessions is null) throw new ArgumentNullException(nameof(sessions));
			if (func is null) throw new ArgumentNullException(nameof(func));
			var results = new List<T>();
			foreach (var session in sessions) results.Add(func(session));
			return results;
		}

		public static (IEnumerable<U> ziped, IEnumerable<S> sessRest, IEnumerable<T> argRest) ZipWith<S, T, U>(this IEnumerable<S> sessions, IEnumerable<T> args, Func<S, T, U> func) where S : Session
		{
			var ss = new List<S>();
			var ts = new List<T>();
			var us = new List<U>();

			var se = sessions.GetEnumerator();
			var ae = args.GetEnumerator();

			while (true)
			{
				if (se.MoveNext())
				{
					if (ae.MoveNext())
					{
						us.Add(func(se.Current, ae.Current));
						continue;
					}
					else
					{
						ss.Add(se.Current);
						while (se.MoveNext())
						{
							ss.Add(se.Current);
						}
						break;
					}
				}
				else
				{
					if (ae.MoveNext())
					{
						ts.Add(ae.Current);
						while (ae.MoveNext())
						{
							ts.Add(ae.Current);
						}
						break;
					}
					else
					{
						break;
					}
				}
			}
			return (us, ss, ts);
		}

		public static (IEnumerable<T1>, IEnumerable<T2>) Unzip<T1, T2>(this IEnumerable<(T1, T2)> ss)
		{
			if (ss is null) throw new ArgumentNullException(nameof(ss));
			var results1 = new List<T1>();
			var results2 = new List<T2>();
			foreach (var s in ss)
			{
				results1.Add(s.Item1);
				results2.Add(s.Item2);
			}
			return (results1, results2);
		}
	}
}