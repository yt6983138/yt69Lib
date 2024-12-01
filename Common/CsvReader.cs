using System.Diagnostics.CodeAnalysis;

namespace yt6983138.Common;
public class CsvReader
{
	private int _currentLine;
	private int _currentColumn;
	private string[] _rows;

	public string[] Rows
	{
		get => this._rows;
		set
		{
			this._rows = value;
			this._currentLine = 0;
			this._currentColumn = 0;
			this.TryReadRow(out _);
		}
	}
	public string Delimiter { get; set; } = ",";

	public int CurrentLine
	{
		get => this._currentLine;
		set
		{
			if (value > this.Rows.Length)
				throw new ArgumentOutOfRangeException(nameof(value));
			this._currentLine = value--;
			this.TryReadRow(out _);
		}
	}
	public int CurrentColumn
	{
		get => this._currentColumn;
		set => this._currentColumn = value > this.CurrentRow.Length
			? throw new ArgumentOutOfRangeException(nameof(value)) : value;
	}

	internal string[] CurrentRow { get; set; }

	public CsvReader(string source, string delimiter)
	{
		if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
		this.Delimiter = delimiter;
		this._rows = source.Replace("\r\n", "\n").Split('\n');
		this.CurrentRow = this.Rows[0].Split(this.Delimiter);
	}

	public bool TryReadRow([NotNullWhen(true)] out string? @out)
	{
		@out = null;
		if (this._currentLine >= this.Rows.Length)
			return false;
		@out = this.Rows[this._currentLine];
		if (string.IsNullOrEmpty(@out)) return false;
		this.CurrentRow = @out.Split(this.Delimiter);
		this._currentLine++;
		this._currentColumn = 0;
		return true;
	}
	public bool TryReadColumn([NotNullWhen(true)] out string? @out)
	{
		@out = null;
		if (this._currentColumn >= this.CurrentRow.Length)
			return false;
		@out = this.CurrentRow[this.CurrentColumn];
		if (string.IsNullOrEmpty(@out)) return false;
		this._currentColumn++;
		return true;
	}

	public string ReadRow()
	{
		if (!this.TryReadRow(out string? str)) throw new InvalidOperationException("No more row left.");
		return str;
	}
	public string ReadColumn()
	{
		if (!this.TryReadColumn(out string? str)) throw new InvalidOperationException("No more column left.");
		return str;
	}
}
