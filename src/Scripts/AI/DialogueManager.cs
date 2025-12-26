using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http; // Keep for other types like StringContent

public partial class DialogueManager : Node
{
    [Export] public string ApiUrl = "http://localhost:8080/dialogue"; // your NPC AI service
    [Export] public bool UseCache = true;

    // Use full namespace to avoid conflict with Godot.HttpClient
    private static readonly System.Net.Http.HttpClient _http = new();

    public override void _Ready()
    {
        AddToGroup("service_dialogue");
    }

    public async Task<DialogueResponse> GetDialogueAsync(DialogueRequest req)
    {
        string state = $"{req.LocationTag}|{req.TimeTag}|{req.PlayerIntent}|{string.Join("||", req.RecentTurns)}|{req.NpcBackstoryCardJson}";
        string key = DialogueCache.MakeKey(req.NpcId, state);

        if (UseCache)
        {
            var cacheFile = DialogueCache.CachePath(key);
            if (FileAccess.FileExists(cacheFile))
            {
                using var f = FileAccess.Open(cacheFile, FileAccess.ModeFlags.Read);
                string json = f.GetAsText();
                var cached = JsonSerializer.Deserialize<DialogueResponse>(json);
                if (cached != null) return cached;
            }
        }

        var payload = JsonSerializer.Serialize(req);
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsync(ApiUrl, content);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Dialogue API error: {ex.Message}");
            return new DialogueResponse
            {
                NpcLine = "…I don’t have words for this right now.",
                PlayerOptions = new() { "Leave", "Try again" }
            };
        }

        var respJson = await resp.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<DialogueResponse>(respJson) ?? new DialogueResponse
        {
            NpcLine = "The stranger stares silently.",
            PlayerOptions = new() { "Leave" }
        };

        if (UseCache)
        {
            if (!DirAccess.DirExistsAbsolute("user://dialogue_cache"))
                DirAccess.MakeDirAbsolute("user://dialogue_cache");

            using var f = FileAccess.Open(DialogueCache.CachePath(key), FileAccess.ModeFlags.Write);
            f.StoreString(JsonSerializer.Serialize(parsed));
        }

        return parsed;
    }
}
