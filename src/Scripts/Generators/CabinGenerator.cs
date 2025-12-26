using Godot;
using System;

public partial class CabinGenerator : Node3D
{
    public override void _Ready()
    {
        GenerateCabin();
    }

    private void GenerateCabin()
    {
        // Root is StaticBody so it blocks player
        var staticBody = new StaticBody3D();
        AddChild(staticBody);

        // 1. Visuals (Logs)
        var visualRoot = new Node3D();
        staticBody.AddChild(visualRoot);
        GenerateLogs(visualRoot);
        GenerateRoof(visualRoot);
        GenerateDoor(visualRoot);

        // 2. Physics (Simplified Walls)
        // Back Wall
        AddBoxCollider(staticBody, new Vector3(6, 3, 0.5f), new Vector3(0, 1.5f, -4));
        // Front Wall Left
        AddBoxCollider(staticBody, new Vector3(2, 3, 0.5f), new Vector3(-2, 1.5f, 4));
        // Front Wall Right
        AddBoxCollider(staticBody, new Vector3(2, 3, 0.5f), new Vector3(2, 1.5f, 4));
        // Front Wall Top (Lintel)
        AddBoxCollider(staticBody, new Vector3(2, 1, 0.5f), new Vector3(0, 2.5f, 4));
        // Left Wall
        AddBoxCollider(staticBody, new Vector3(0.5f, 3, 8), new Vector3(-3, 1.5f, 0));
        // Right Wall
        AddBoxCollider(staticBody, new Vector3(0.5f, 3, 8), new Vector3(3, 1.5f, 0));

        // 3. Interaction (Claimable Property)
        // Add an Area3D on Layer 4 (Interaction)
        // Attach ClaimableProperty script to it
        var interactArea = new Area3D();
        interactArea.CollisionLayer = 1 << 3; // Layer 4
        interactArea.CollisionMask = 0;

        // Add collision shape for interaction at the door
        var interactShape = new CollisionShape3D();
        interactShape.Shape = new BoxShape3D() { Size = new Vector3(2, 2, 2) };
        interactShape.Position = new Vector3(0, 1, 5); // Just outside door
        interactArea.AddChild(interactShape);

        // Add the script
        var claimScript = new ClaimableProperty(); // It is a Node3D
        // Actually, we want the collider to BE the interactable or have it.
        // If I make the Area3D have the script, I can't just 'new ClaimableProperty()' because it's a script not an Area3D.
        // I will add ClaimableProperty as a child, and the Area3D needs to point to it?
        // Or simpler: The InteractionSystem usually does `collider.GetParent()` or `collider is IInteractable`.
        // If `ClaimableProperty` inherits Area3D it would be easiest. But it inherits Node3D.
        // So I will make the Area3D the child of ClaimableProperty.
        // And the Raycast logic usually hits the Area, then checks parent?
        // Let's assume standard Godot pattern: Raycast hits Area. If Area implements IInteractable, good. If not, check parent.
        // But Area3D cannot have a script replaced at runtime easily C#.
        // I'll stick to: ClaimableProperty (Node3D) -> Area3D -> CollisionShape.
        // AND I will add a script to the Area3D that forwards to parent? No, that's complex.

        // BETTER: Make the ClaimableProperty the StaticBody itself? No.

        // Let's look at `ClaimableProperty.cs` again. It inherits Node3D.
        // I will add `ClaimableProperty` node.
        staticBody.AddChild(claimScript);
        claimScript.Name = "ClaimableProperty";
        claimScript.PropertyId = "Cabin_" + GetInstanceId(); // Unique ID

        // Add the Area3D as a child of the ClaimScript.
        // BUT the Raycast hits the Area.
        // If the Raycast hits the Area, `collider` is the Area.
        // Does the player's interaction code look for `collider.GetParent<IInteractable>()`?
        // I should check InteractionSystem/PlayerRaycast.
        // Since I can't check right now easily without context switching, I'll implement the "collider holds the script" pattern.
        // I will change ClaimableProperty to inherit Area3D in the next step if needed, but for now I will add the Area3D to the ClaimableProperty
        // and assume the interaction system looks up.
        // Wait, I am the one writing the interaction system or usage.
        // I don't see InteractionSystem.cs in my file list memory, but I added it?
        // Ah, I added `src/Scripts/Systems/InteractionSystem.cs`? No, I added `QuestManager` etc.
        // I haven't written the Player Controller's interaction logic yet! I need to write that logic in the PlayerGenerator or Main.

        // So I will decide now: The Interaction Raycast will look for IInteractable on the collider, then the collider's parent.
        claimScript.AddChild(interactArea);

    }

    private void GenerateLogs(Node parent)
    {
        float logDiameter = 0.3f;
        int logCount = (int)(3.0f / logDiameter);

        for (int i = 0; i < logCount; i++)
        {
            float y = i * logDiameter;
            CreateLog(parent, new Vector3(0, y, -4), new Vector3(6, 0, 0), logDiameter);

            if (i < 7)
            {
                CreateLog(parent, new Vector3(-2, y, 4), new Vector3(2, 0, 0), logDiameter);
                CreateLog(parent, new Vector3(2, y, 4), new Vector3(2, 0, 0), logDiameter);
            }
            else
            {
                CreateLog(parent, new Vector3(0, y, 4), new Vector3(6, 0, 0), logDiameter);
            }
        }

        for (int i = 0; i < logCount; i++)
        {
            float y = i * logDiameter + (logDiameter * 0.5f);
            CreateLog(parent, new Vector3(-3, y, 0), new Vector3(0, 0, 8), logDiameter);
            CreateLog(parent, new Vector3(3, y, 0), new Vector3(0, 0, 8), logDiameter);
        }
    }

    private void GenerateRoof(Node parent)
    {
        var leftRafter = new MeshInstance3D();
        leftRafter.Mesh = new BoxMesh() { Size = new Vector3(4.5f, 0.2f, 9f) };
        leftRafter.Position = new Vector3(-1.5f, 4f, 0);
        leftRafter.RotationDegrees = new Vector3(0, 0, 45);
        leftRafter.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.2f, 0.1f, 0.05f) };
        parent.AddChild(leftRafter);

        var rightRafter = new MeshInstance3D();
        rightRafter.Mesh = new BoxMesh() { Size = new Vector3(4.5f, 0.2f, 9f) };
        rightRafter.Position = new Vector3(1.5f, 4f, 0);
        rightRafter.RotationDegrees = new Vector3(0, 0, -45);
        rightRafter.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.2f, 0.1f, 0.05f) };
        parent.AddChild(rightRafter);
    }

    private void GenerateDoor(Node parent)
    {
        var door = new MeshInstance3D();
        door.Mesh = new BoxMesh() { Size = new Vector3(1.2f, 2.1f, 0.1f) };
        door.Position = new Vector3(0, 1.05f, 4.0f);
        door.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.5f, 0.3f, 0.2f) };
        parent.AddChild(door);

        var handle = new MeshInstance3D();
        handle.Mesh = new SphereMesh() { Radius = 0.05f };
        handle.Position = new Vector3(0.4f, 1.0f, 4.1f);
        handle.MaterialOverride = new StandardMaterial3D() { AlbedoColor = new Color(0.8f, 0.8f, 0.2f), Metallic = 1.0f };
        parent.AddChild(handle);
    }

    private void CreateLog(Node parent, Vector3 center, Vector3 axis, float diameter)
    {
        var mesh = new CylinderMesh();
        mesh.TopRadius = diameter / 2;
        mesh.BottomRadius = diameter / 2;
        mesh.Height = axis.Length();

        var inst = new MeshInstance3D();
        inst.Mesh = mesh;
        inst.Position = center;

        if (axis.X > axis.Z)
            inst.RotationDegrees = new Vector3(0, 0, 90);
        else
            inst.RotationDegrees = new Vector3(90, 0, 0);

        inst.MaterialOverride = new StandardMaterial3D()
        {
            AlbedoColor = new Color(0.4f, 0.25f, 0.15f),
            Roughness = 1.0f
        };

        parent.AddChild(inst);
    }

    private void AddBoxCollider(StaticBody3D body, Vector3 size, Vector3 pos)
    {
        var shape = new CollisionShape3D();
        shape.Shape = new BoxShape3D() { Size = size };
        shape.Position = pos;
        body.AddChild(shape);
    }
}
