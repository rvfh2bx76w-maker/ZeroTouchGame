using System.Collections.Generic;

public sealed class DialogueRequest
{
    public string WorldSeed { get; set; } = "";
    public string NpcId { get; set; } = "";
    public string FactionId { get; set; } = "";
    public string LocationTag { get; set; } = "";
    public string TimeTag { get; set; } = "";
    public string PlayerIntent { get; set; } = "";
    public string LoreBible { get; set; } = "";
    public string NpcBackstoryCardJson { get; set; } = "";
    public List<string> RecentTurns { get; set; } = new();
}

public sealed class DialogueResponse
{
    public string NpcLine { get; set; } = "";
    public List<string> PlayerOptions { get; set; } = new();
    public List<string> MemoryTags { get; set; } = new();
}
