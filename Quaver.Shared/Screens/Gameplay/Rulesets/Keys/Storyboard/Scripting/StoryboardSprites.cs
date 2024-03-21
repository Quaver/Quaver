using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardSprites
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut;

    public StoryboardSprites(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    public Sprite Receptor(int lane) => Shortcut.GameplayPlayfieldKeysStage.Receptors[lane - 1];
    public Sprite BgMask => Shortcut.GameplayPlayfieldKeysStage.BgMask;
    public Sprite Background => Shortcut.GameplayScreenView.Background;
    public Container ForegroundContainer => Shortcut.GameplayPlayfieldKeys.ForegroundContainer;

    public HitObjectManagerKeys HitObjectManagerKeys => (HitObjectManagerKeys)Shortcut.GameplayScreen.Ruleset.HitObjectManager;

    public Sprite CreateSprite(Drawable parent, Texture2D texture2D, ScalableVector2 position, ScalableVector2 size)
    {
        return new Sprite
        {
            Parent = parent,
            Image = texture2D,
            Position = position,
            Size = size
        };
    }
}