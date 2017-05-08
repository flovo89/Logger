namespace Logger.Common.Collections.Generic
{
	public delegate TKey KeyResolveCallback <out TKey, in TItem> (TItem item);
}
