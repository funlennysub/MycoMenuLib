# MycoMenuLib

Reusable settings-row helpers for Mycopunk mods, using the game's own prefabs.

## Requirements

- BepInEx 5 (BepInExPack_Mycopunk)
- Add a hard dependency in your plugin:

```csharp
[BepInDependency("dev.funlennysub.MycoMenuLib")]
```

## Usage

```csharp
using MycoMenuLib;

ModUI.AddCategory(items, "My Mod");

ModUI.AddToggle(items, "mymod.enabled", "Enabled", enabled,
    value => { /* ... */ });

ModUI.AddFloatSlider(items, "mymod.scale", "Scale", scale, 0.1f, 2f, 2,
    value => { /* ... */ });

ModUI.AddIntSlider(items, "mymod.count", "Count", count, 1, 20,
    value => { /* ... */ });

ModUI.AddDropdown(items, "mymod.mode", "Mode", options, currentIndex,
    index => { /* ... */ });

ModUI.AddTextInput(items, "mymod.name", "Name", name,
    value => { /* ... */ });

ModUI.AddInput(items, "mymod.secret", "Token", token,
    new InputFieldOptions
    {
        Type = ModInputType.Password,
        ShowRevealButton = true,
        CharacterLimit = 128,
    },
    value => { /* ... */ });

ModUI.AddInput(items, "mymod.notes", "Notes", notes,
    new InputFieldOptions
    {
        Multiline = true,
        Height = 150f,
        Placeholder = "Optional notes...",
    },
    value => { /* ... */ });

ModUI.AddKeybind(items, "mymod.key", "Hotkey", key,
    (code, modifiers) => code,
    code => code.ToString(),
    value => { /* ... */ });

ModUI.AddDivider(items);

ModUI.AddRowAction(rowControl, sprite, 0, () => { /* ... */ });
```
