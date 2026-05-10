namespace Zilean.Database.Services;

public readonly record struct StoreResult(int Stored, long PopulateMs, long MatchMs, long UpsertMs);
