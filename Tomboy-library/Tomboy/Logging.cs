//
//  Logging.cs
//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2014 Timo Dörr
//
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Linq;
using Tomboy;

namespace Tomboy
{
	public interface ILogger
	{
		void Info (params object[] message);
		void Warn (params object[] message);
		void Error (params object[] message);
		void Fatal (params object[] message);
		void Debug (params object[] message);
	}

	public class DummyLogger : ILogger
	{
		protected enum LogLevel { INFO, WARN, ERROR, FATAL, DEBUG };
		protected virtual void printNotice (LogLevel loglevel, params object[] message)
		{
		}

		#region ILogger implementation
		public void Info (params object[] message)
		{
			printNotice (LogLevel.INFO, message);
		}
		public void Warn (params object[] message)
		{
			printNotice (LogLevel.WARN, message);
		}
		public void Error (params object[] message)
		{
			printNotice (LogLevel.ERROR, message);
		}
		public void Fatal (params object[] message)
		{
			printNotice (LogLevel.FATAL, message);
		}
		public void Debug (params object[] message)
		{
			printNotice (LogLevel.DEBUG, message);
		}
		#endregion
	}
	
	public class ConsoleLogger : DummyLogger
	{
		public ConsoleLogger ()
		{
		}
		protected override void printNotice (DummyLogger.LogLevel loglevel, params object[] message)
		{
			string level = loglevel.ToString ();
			string mainstring = message.First ().ToString ();
			object[] prams = message.Skip (1).ToArray<object> ();

			var output_message = string.Format (mainstring, prams);
			Console.WriteLine ("[{0}] {1}", level, output_message);
		}
	}

}