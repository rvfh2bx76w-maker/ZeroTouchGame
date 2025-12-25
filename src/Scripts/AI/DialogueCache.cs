using Godot;
using System.Security.Cryptography;
using System.Text;

public static class DialogueCache
{
    public static string MakeKey(string npcId, string state)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(npcId + "|" + state));
        return System.Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string CachePath(string key) => $"user://dialogue_cache/{key}.json";
}
