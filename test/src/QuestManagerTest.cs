using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class QuestManagerTest
{
    private QuestManager _manager = null!;

    [Before]
    public void Setup()
    {
        _manager = new QuestManager();
        _manager._Ready(); // Simulates Godot's Ready call
    }

    [TestCase]
    public void TestQuestCycle()
    {
        // 1. Verify initial state
        var state = _manager.GetQuestState("quest_cabin");
        AssertThat(state).IsEqual(QuestState.NotStarted);

        // 2. Start Quest
        _manager.StartQuest("quest_cabin");
        state = _manager.GetQuestState("quest_cabin");
        AssertThat(state).IsEqual(QuestState.Active);

        // 3. Complete Quest
        _manager.CompleteQuest("quest_cabin");
        state = _manager.GetQuestState("quest_cabin");
        AssertThat(state).IsEqual(QuestState.Completed);
    }

    [TestCase]
    public void TestUnknownQuest()
    {
        var state = _manager.GetQuestState("non_existent_quest");
        AssertThat(state).IsEqual(QuestState.NotStarted);
    }
}
