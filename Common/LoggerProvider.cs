using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yt6983138.Common;
public sealed class LoggerProvider : ILoggerProvider
{
	private readonly IDisposable _onChangeToken;
	private LoggerConfiguration _currentConfig;
	private readonly ConcurrentDictionary<string, Logger> _loggers =
		new(StringComparer.OrdinalIgnoreCase);

	public LoggerProvider(IOptionsMonitor<LoggerConfiguration> config)
	{
		this._currentConfig = config.CurrentValue;
		this._onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig)!;
	}
	public ILogger CreateLogger(string name)
	{
		return this._loggers.GetOrAdd(name, name =>
						   new Logger(this._currentConfig));
	}
	void IDisposable.Dispose()
	{

	}
}
