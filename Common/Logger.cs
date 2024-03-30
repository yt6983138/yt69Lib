using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yt6983138.Common;

public sealed class StateTracker : IDisposable
{
	public bool IsDisposed { get; private set; } = false;
	public void Dispose()
	{
		IsDisposed = true;
	}
}
public class LoggerConfiguration
{
	public string? PathToWrite { get; set; }
	public List<LogLevel>? Disabled { get; set; }
}
public class LogEventArgs : EventArgs
{
	public LogLevel Type;
	public string Message;
	public Exception? Exception;

	public LogEventArgs(LogLevel type, string message, Exception? ex = null)
	{
		Type = type;
		Message = message;
		Exception = ex;
	}
}
public class Logger : ILogger
{
	private static readonly object _lock = new();

	public List<string> AllLogs { get; private set; } = new();
	public string LatestLogMessage { get; private set; } = string.Empty;
	public string LatestLogMessageUnformatted { get; private set; } = string.Empty;
	// [time] [type] [scope] (id) message
	public string LogFormat { get; set; } = "[{0}] [{1}] [{2}] ({3}) {4}\n";
	public string ExceptionFormat { get; set; } = "{0}\nInner: {1}\nStack Trace:\n{2}";

	public List<LogLevel> Disabled { get; set; } = new();

	public delegate void LogEventHandler(object? sender, LogEventArgs args);
	public event LogEventHandler? OnLog;

	private FileStream? _logStream;
	public Logger(string? pathToWrite = null)
	{
		try
		{
			_logStream = new(pathToWrite!, FileMode.Append, FileAccess.Write);
		}
		catch
		{
			_logStream = null;
		}
	}
	public Logger(LoggerConfiguration config) : this(config.PathToWrite)
	{
		if (config.Disabled is not null)
			this.Disabled = config.Disabled;
	}

	public void Log<TState>(LogLevel type, string message, EventId id, TState state, Exception? ex = null)
	{
		OnLog?.Invoke(this, new(type, message, ex));
		if (Disabled.Contains(type)) return;
		string formatted = string.Format(this.LogFormat, DateTime.Now, type.ToString(), typeof(TState).Name, id.ToString(), message);
		lock (_lock)
		{
			Console.Write(formatted);
			this.AllLogs.Add(formatted);
			this.LatestLogMessageUnformatted = message;
			this.LatestLogMessage = formatted;
			WriteMessage(formatted);
		}
	}
	public void Log<TState>(LogLevel type, EventId id, TState state, Exception ex)
	{
		string compiled = string.Format(
				this.ExceptionFormat,
				ex.Message,
				ex.InnerException == null ? "Empty" : ex.InnerException.Message,
				ex.StackTrace
				);
		Log(type, compiled, id, state);
	}
	public void Log<TState>(LogLevel level, EventId id, TState state, Exception? ex, Func<TState, Exception?, string> formatter)
	{
		string formatted = formatter(state, ex);
		if (ex is null)
		{
			Log(level, formatted, id, state, ex);
		}
		else
		{
			Log(level, id, state, ex);
		}
	}
	public void WriteMessage(string message)
	{
		byte[] buf = Encoding.UTF8.GetBytes(message);
		_logStream?.Write(buf);
	}
	public bool IsEnabled(LogLevel level)
		=> !Disabled.Contains(level);
	public IDisposable BeginScope<TState>(TState obj) where TState : notnull
	{
		StateTracker tracker = new();

		return tracker;
	}
}
