namespace MariesWonderland.Data;

/// <summary>
/// Owns the current <see cref="MasterDataSnapshot"/>. Requests take the snapshot once at the start
/// (see the scoped registrations), so swapping in a reloaded snapshot never affects a request in flight.
/// </summary>
public sealed class MasterDataHolder(MasterDataSnapshot initial)
{
    private MasterDataSnapshot _current = initial;

    public MasterDataSnapshot Current => Volatile.Read(ref _current);

    public void Replace(MasterDataSnapshot snapshot) => Volatile.Write(ref _current, snapshot);
}
