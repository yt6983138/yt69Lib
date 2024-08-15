namespace yt6983138.Common;
public static class Extensions
{
	public static T EnsureNotNull<T>(this T? value) where T : class
	{
		ArgumentNullException.ThrowIfNull(value, nameof(value));
		return value;
	}
	public static T EnsureNotNull<T>(this Nullable<T> value) where T : struct
	{
		ArgumentNullException.ThrowIfNull(value, nameof(value));
		return value.Value;
	}
}
