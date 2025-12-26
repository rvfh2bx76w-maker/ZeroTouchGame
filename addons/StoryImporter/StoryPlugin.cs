#if TOOLS
using Godot;

[Tool]
public partial class StoryPlugin : EditorPlugin
{
    private StoryImporter? _importer;

    public override void _EnterTree()
    {
        _importer = new StoryImporter();
        AddImportPlugin(_importer);
    }

    public override void _ExitTree()
    {
        if (_importer != null)
        {
            RemoveImportPlugin(_importer);
            _importer = null;
        }
    }
}
#endif
