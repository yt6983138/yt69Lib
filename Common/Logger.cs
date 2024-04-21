﻿using Microsoft.Extensions.Logging;
using System.Text;

namespace yt6983138.Common;

public sealed class StateTracker : IDisposable
{
	public bool IsDisposed { get; private set; } = false;
	public void Dispose()
	{
		this.IsDisposed = true;
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
		this.Type = type;
		this.Message = message;
		this.Exception = ex;
	}
}
public class Logger : ILogger
{
	private static readonly object _lock = new();

	public static readonly List<string> AllLogs = new();
	public List<LogLevel> Disabled { get; set; } = new();
	/// <summary>
	/// set to -1 to disable
	/// </summary>
	public static int MaxLogCapacity { get; set; } = 16384;

	public string LatestLogMessage { get; private set; } = string.Empty;
	public string LatestLogMessageUnformatted { get; private set; } = string.Empty;
	// [time] [type] [scope] (id) message
	public string LogFormat { get; set; } = "[{0}] [{1}] [{2}] ({3}) {4}\n";
	public string ExceptionFormat { get; set; } = "{0}\nInner: {1}\nStack Trace:\n{2}";

	public readonly static Logger Shared = new();

	public delegate void LogEventHandler(object? sender, LogEventArgs args);
	public event LogEventHandler? OnBeforeLog;
	public event LogEventHandler? OnAfterLog;

	private FileStream? _logStream;
	public Logger(string? pathToWrite = null)
	{
		try
		{
			this._logStream = new(pathToWrite!, FileMode.Append, FileAccess.Write);
		}
		catch
		{
			this._logStream = null;
		}
	}
	public Logger(LoggerConfiguration config) : this(config.PathToWrite)
	{
		if (config.Disabled is not null)
			this.Disabled = config.Disabled;
	}

	public void Log<TState>(LogLevel type, string message, EventId id, TState state, Exception? ex = null)
	{
		OnBeforeLog?.Invoke(this, new(type, message, ex));
		if (this.Disabled.Contains(type)) return;
		string formatted = string.Format(this.LogFormat, DateTime.Now, type.ToString(), typeof(TState).Name, id.ToString(), message);
		if (ex is not null)
			formatted += string.Format(
				this.ExceptionFormat,
				ex.Message,
				ex.InnerException == null ? "Empty" : ex.InnerException.Message,
				ex.StackTrace
				);
		lock (_lock)
		{
			Console.Write(formatted);
			AllLogs.Add(formatted);
			if (AllLogs.Count > MaxLogCapacity && MaxLogCapacity > 0)
			{
				AllLogs.RemoveAt(AllLogs.Count - 1);
			}
			this.LatestLogMessageUnformatted = message;
			this.LatestLogMessage = formatted;
			this.WriteMessage(formatted);
		}
		OnAfterLog?.Invoke(this, new(type, message, ex));
	}
	public void Log<TState>(LogLevel type, EventId id, TState state, Exception ex)
	{
		string compiled = string.Format(
				this.ExceptionFormat,
				ex.Message,
				ex.InnerException == null ? "Empty" : ex.InnerException.Message,
				ex.StackTrace
				);
		this.Log(type, compiled, id, state);
	}
	public void Log<TState>(LogLevel level, EventId id, TState state, Exception? ex, Func<TState, Exception?, string> formatter)
	{
		string formatted = formatter(state, ex);
		if (ex is null)
		{
			this.Log(level, formatted, id, state, ex);
		}
		else
		{
			this.Log(level, id, state, ex);
		}
	}
	public void WriteMessage(string message)
	{
		byte[] buf = Encoding.UTF8.GetBytes(message);
		this._logStream?.Write(buf);
	}
	public bool IsEnabled(LogLevel level)
		=> !this.Disabled.Contains(level);
	public IDisposable BeginScope<TState>(TState obj) where TState : notnull
	{
		StateTracker tracker = new();

		return tracker;
	}
}
