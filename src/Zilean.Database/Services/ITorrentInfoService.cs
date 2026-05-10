namespace Zilean.Database.Services;

public interface ITorrentInfoService
{
    Task<StoreResult> StoreTorrentInfo(List<TorrentInfo> torrents, int batchSize = 5000);
    Task<TorrentInfo[]> SearchForTorrentInfoByOnlyTitle(string query);
    Task<TorrentInfo[]> SearchForTorrentInfoFiltered(TorrentInfoFilter filter, int? limit = null);
    Task<HashSet<string>> GetExistingInfoHashesAsync(List<string> infoHashes);
    Task<HashSet<string>> GetBlacklistedItems();
    Task VaccumTorrentsIndexes(CancellationToken cancellationToken);
}
