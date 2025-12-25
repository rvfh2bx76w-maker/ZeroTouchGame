using Godot;
using System.Collections.Generic;

public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed
}

public class QuestData
{
    public string QuestId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public QuestState State { get; set; } = QuestState.NotStarted;

    // A simplified step tracker: 0 = first step
    public int CurrentStepIndex { get; set; } = 0;
}

public partial class QuestManager : Node
{
    // Dictionary of QuestID -> QuestData
    private Dictionary<string, QuestData> _quests = new Dictionary<string, QuestData>();

    private SaveService _saveService;

    public override void _Ready()
    {
        AddToGroup("service_quest");

        // In a real implementation, we would load Quest Definitions from JSON resources
        // For vertical slice, we can pre-populate a test quest.
        DefineQuest("quest_cabin", "Claim the Cabin", "Find a way to claim the abandoned cabin.");
    }

    private void DefineQuest(string id, string title, string description)
    {
        if (!_quests.ContainsKey(id))
        {
            _quests[id] = new QuestData
            {
                QuestId = id,
                Title = title,
                Description = description
            };
        }
    }

    public void StartQuest(string questId)
    {
        if (_quests.ContainsKey(questId) && _quests[questId].State == QuestState.NotStarted)
        {
            _quests[questId].State = QuestState.Active;
            GD.Print($"Quest Started: {_quests[questId].Title}");
        }
    }

    public void CompleteQuest(string questId)
    {
        if (_quests.ContainsKey(questId) && _quests[questId].State == QuestState.Active)
        {
            _quests[questId].State = QuestState.Completed;
            GD.Print($"Quest Completed: {_quests[questId].Title}");
            // Grant rewards...
        }
    }

    public QuestState GetQuestState(string questId)
    {
        if (_quests.ContainsKey(questId))
            return _quests[questId].State;
        return QuestState.NotStarted;
    }
}
