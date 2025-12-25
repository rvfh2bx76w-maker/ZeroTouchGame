using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using System.Collections.Generic;

[TestSuite]
public class PropertySystemTest
{
    private PropertySystem _system;

    [Before]
    public void Setup()
    {
        _system = new PropertySystem();
        // Since it's a singleton in practice, we test the instance logic
    }

    [TestCase]
    public void TestOwnership()
    {
        string propId = "cabin_01";

        // 1. Initially not owned
        AssertThat(_system.IsOwned(propId)).IsFalse();

        // 2. Set Owned
        _system.SetOwned(propId, true);
        AssertThat(_system.IsOwned(propId)).IsTrue();

        // 3. Revoke
        _system.SetOwned(propId, false);
        AssertThat(_system.IsOwned(propId)).IsFalse();
    }

    [TestCase]
    public void TestSnapshotLoad()
    {
        _system.SetOwned("house_1", true);
        _system.SetOwned("house_2", true);

        var snapshot = _system.SnapshotOwned();
        AssertThat(snapshot).Contains("house_1", "house_2");

        // Clear and load
        _system.SetOwned("house_1", false);
        _system.SetOwned("house_2", false);
        AssertThat(_system.IsOwned("house_1")).IsFalse();

        _system.LoadOwned(snapshot);
        AssertThat(_system.IsOwned("house_1")).IsTrue();
    }
}
