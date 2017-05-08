namespace Logger.Common.Collections.Generic
{
	public interface IKeyResolver <out TKey, in TItem>
	{
		TKey GetKeyForItem (TItem item);
	}
}
