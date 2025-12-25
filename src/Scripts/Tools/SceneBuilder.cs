using Godot;
using System;

#if TOOLS
[Tool]
#endif
public partial class SceneBuilder : Node
{
    // This script is intended to be run once to generate the Main.tscn file

    public void BuildMainScene()
    {
        var main = new Node();
        main.Name = "Main";

        // 1. Services
        var services = new Node();
        services.Name = "Services";
        main.AddChild(services);

        // SceneFlow
        var sceneFlow = new Node();
        sceneFlow.Name = "SceneFlow";
        sceneFlow.SetScript(GD.Load<Script>("res://src/Scripts/Systems/SceneFlow.cs"));
        sceneFlow.Set("OverworldRootPath", new NodePath("../../World/OverworldRoot"));
        sceneFlow.Set("DungeonRootPath", new NodePath("../../World/DungeonRoot"));
        sceneFlow.Set("FadeOverlayPath", new NodePath("../../UI/FadeOverlay"));
        services.AddChild(sceneFlow);

        AddService(services, "SaveService", "res://src/Scripts/Autoload/SaveService.cs");
        AddService(services, "FactionSystem", "res://src/Scripts/Systems/FactionSystem.cs", "service_faction");
        AddService(services, "RelationshipSystem", "res://src/Scripts/Systems/RelationshipSystem.cs", "service_relationship");
        AddService(services, "PropertySystem", "res://src/Scripts/Systems/PropertySystem.cs", "service_property");
        AddService(services, "DialogueManager", "res://src/Scripts/AI/DialogueManager.cs", "service_dialogue");

        // 2. World
        var world = new Node();
        world.Name = "World";
        main.AddChild(world);

        var overworldRoot = new Node3D();
        overworldRoot.Name = "OverworldRoot";
        world.AddChild(overworldRoot);

        var gameplayRoot = new Node3D();
        gameplayRoot.Name = "GameplayRoot";
        overworldRoot.AddChild(gameplayRoot);

        // OverworldStream
        var overworldStream = new Node3D();
        overworldStream.Name = "OverworldStream";
        overworldStream.SetScript(GD.Load<Script>("res://src/Scripts/World/OverworldStream.cs"));
        overworldStream.Set("PlayerPath", new NodePath("../../../../PlayerRoot/Player"));
        overworldStream.Set("GameplayRootPath", new NodePath(".."));
        overworldStream.Set("GameplayChunkScene", GD.Load<PackedScene>("res://src/Scenes/GameplayChunk.tscn"));
        gameplayRoot.AddChild(overworldStream);

        // TerrainRoot
        var terrainRoot = new Node3D();
        terrainRoot.Name = "TerrainRoot";
        terrainRoot.SetScript(GD.Load<Script>("res://src/Scripts/World/ProceduralTerrain.cs"));
        overworldRoot.AddChild(terrainRoot);

        // DungeonRoot
        var dungeonRoot = new Node3D();
        dungeonRoot.Name = "DungeonRoot";
        dungeonRoot.SetScript(GD.Load<Script>("res://src/Scripts/World/ProceduralDungeon.cs"));
        dungeonRoot.Visible = false;
        world.AddChild(dungeonRoot);

        // 3. PlayerRoot
        var playerRoot = new Node3D();
        playerRoot.Name = "PlayerRoot";
        main.AddChild(playerRoot);

        var player = new CharacterBody3D();
        player.Name = "Player";
        player.CollisionLayer = 2; // Player layer
        player.CollisionMask = 1 | 3 | 4; // World, Enemies, Interactables
        player.SetScript(GD.Load<Script>("res://src/Scripts/Player/SimplePlayerController.cs"));
        playerRoot.AddChild(player);

        var capsule = new CollisionShape3D();
        capsule.Shape = new CapsuleShape3D();
        capsule.Position = new Vector3(0, 1, 0);
        player.AddChild(capsule);

        var head = new Node3D();
        head.Name = "Head";
        head.Position = new Vector3(0, 1.7f, 0);
        player.AddChild(head);

        var camera = new Camera3D();
        camera.Name = "Camera3D";
        head.AddChild(camera);

        AddNodeWithScript(player, "PlayerStats", "res://src/Scripts/Player/PlayerStats.cs");

        // PlayerCombat
        var playerCombat = new Node();
        playerCombat.Name = "PlayerCombat";
        playerCombat.SetScript(GD.Load<Script>("res://src/Scripts/Player/PlayerCombat.cs"));
        playerCombat.Set("CameraPath", new NodePath("../Head/Camera3D"));
        player.AddChild(playerCombat);

        // 4. UI
        var ui = new CanvasLayer();
        ui.Name = "UI";
        main.AddChild(ui);

        var fade = new ColorRect();
        fade.Name = "FadeOverlay";
        fade.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        fade.Color = new Color(0, 0, 0, 0);
        fade.MouseFilter = Control.MouseFilterEnum.Ignore;
        ui.AddChild(fade);

        // 5. Environment
        var env = new WorldEnvironment();
        env.Name = "WorldEnvironment";
        var envRes = new Godot.Environment();
        envRes.BackgroundMode = Godot.Environment.BGMode.Sky;
        envRes.Sky = new Sky();
        envRes.Sky.SkyMaterial = new ProceduralSkyMaterial();
        envRes.TonemapMode = Godot.Environment.ToneMapper.Filmic;
        env.Environment = envRes;
        main.AddChild(env);

        var sun = new DirectionalLight3D();
        sun.Name = "Sun";
        sun.RotationDegrees = new Vector3(-45, 45, 0);
        sun.ShadowEnabled = true;
        main.AddChild(sun);

        var scene = new PackedScene();
        scene.Pack(main);
        ResourceSaver.Save(scene, "res://src/Scenes/Main.tscn");
        GD.Print("Main.tscn created successfully.");
    }

    private void AddService(Node parent, string name, string scriptPath, string group = "")
    {
        var node = new Node();
        node.Name = name;
        node.SetScript(GD.Load<Script>(scriptPath));
        if (!string.IsNullOrEmpty(group))
            node.AddToGroup(group);
        parent.AddChild(node);
    }

    private void AddNodeWithScript(Node parent, string name, string scriptPath)
    {
        var node = new Node();
        node.Name = name;
        node.SetScript(GD.Load<Script>(scriptPath));
        parent.AddChild(node);
    }
}
