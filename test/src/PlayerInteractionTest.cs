using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public partial class PlayerInteractionTest
{
    // A mock interactable class
    private partial class MockInteractable : Node, IInteractable
    {
        public bool Interacted = false;
        public string GetInteractionPrompt() => "Mock Prompt";
        public void Interact(Node interactor) { Interacted = true; }
        public bool CanInteract(Node interactor) => true;
    }

    [TestCase]
    public void TestInteractionInterface()
    {
        var mock = new MockInteractable();
        var player = new Node(); // Dummy player

        AssertThat(mock.GetInteractionPrompt()).IsEqual("Mock Prompt");
        AssertThat(mock.CanInteract(player)).IsTrue();

        mock.Interact(player);
        AssertThat(mock.Interacted).IsTrue();
    }
}
