using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.GameModes.Keys
{
    internal class KeysHitObject : HitObject
    {
        /// <summary>
        ///     Reference to the Keys ruleset.
        /// </summary>
        private GameModeRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Reference to the actual playfield.
        /// </summary>
        private KeysPlayfield Playfield { get; set; }

        /// <summary>
        ///     If the note is a long note.
        ///     In .qua format, long notes are defined as if the end time is greater than 0.
        /// </summary>
        internal bool IsLongNote => Info.EndTime > 0;

        /// <summary>
        ///     The X position of the object.
        /// </summary>
        internal float PositionX => Playfield.Stage.Receptors[Info.Lane - 1].PosX;
        
        /// <summary>
       ///     The Y position of the HitObject.
       /// </summary>
        internal float PositionY { get; set; }

        /// <summary>
        ///     The width of the object.
        /// </summary>
        internal float Width { get; set; }

        /// <summary>
        ///     The Y-Offset from the receptor.
        /// </summary>
        internal float OffsetYFromReceptor { get; set; }

        /// <summary>
        ///     The long note Y offset from the receptor.        
        /// </summary>
        internal float LongNoteOffsetYFromReceptor { get; set; }

        /// <summary>
        ///     The initial size of this object's long note.
        /// </summary>
        internal ulong InitialLongNoteSize { get; set; }

        /// <summary>
        ///     The current size of this object's long note.
        /// </summary>
        internal ulong CurrentLongNoteSize { get; set; }

        /// <summary>
        ///      The offset of the long note body from the hit object.
        /// </summary>
        private float LongNoteBodyOffset { get; set; }

        /// <summary>
        ///     The offset of the hold end from hold body.
        /// </summary>
        private float LongNoteEndOffset { get; set; }

        /// <summary>
        ///     The actual HitObject sprite.
        /// </summary>
        private Sprite HitObjectSprite { get; set; }

        /// <summary>
        ///     The hold body sprite for long notes.
        /// </summary>
        internal AnimatableSprite LongNoteBodySprite { get; set; }

         /// <summary>
        ///     The hold end sprite for long notes.
        /// </summary>
        private Sprite LongNoteEndSprite { get; set; }

        /// <summary>
        ///     The SpriteEffects. Flips the image horizontally if we are using upscroll.
        /// </summary>
        private static SpriteEffects Effects => !ConfigManager.DownScroll4K.Value && GameBase.Skin.Keys[GameBase.SelectedMap.Mode].FlipNoteImagesOnUpscroll 
                                                    ? SpriteEffects.FlipVertically : SpriteEffects.None;

        /// <summary>
        ///     The index of this object of the receptor's lane.
        /// </summary>
        private int Index => Info.Lane - 1;

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public KeysHitObject(GameModeRulesetKeys ruleset, HitObjectInfo info) : base(info) => Ruleset = ruleset;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        internal override void InitializeSprite(IGameplayPlayfield playfield)
        {
            // Get the KeysPlayfield instance rather than just the interface type.
            Playfield = (KeysPlayfield) playfield;
    
            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(PositionX, PositionY),
                SpriteEffect = Effects,
                Image = GetHitObjectTexture(),
            };
            
            // Update hit body's size to match image ratio
            HitObjectSprite.Size = new UDim2D(Playfield.LaneSize, Playfield.LaneSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);
            LongNoteBodyOffset = HitObjectSprite.SizeY / 2;
            
            if (IsLongNote)
                CreateLongNote();

            // We set the parent of the HitObjectSprite **AFTER** we create the long note
            // so that the body of the long note isn't drawn over the object.
            HitObjectSprite.Parent = Playfield.Stage.HitObjectContainer;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal override void Destroy()
        {            
            HitObjectSprite.Destroy();
            
            if (IsLongNote)
            {
                LongNoteBodySprite.Destroy();
                LongNoteEndSprite.Destroy();
            }
        }
        
        /// <summary>
        ///     Creates the long note sprite.
        /// </summary>
        private void CreateLongNote()
        {
            // Get the long note bodies to use.
            var bodies = GameBase.Skin.Keys[Playfield.Map.Mode].NoteHoldBodies[Index];
     
            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(Playfield.LaneSize, InitialLongNoteSize),
                Position = new UDim2D(PositionX, PositionY),
                Parent = Playfield.Stage.HitObjectContainer
            };
                                  
            // Create the Hold End
            LongNoteEndSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(PositionX, PositionY),
                Size = new UDim2D(Playfield.LaneSize),
                Parent = Playfield.Stage.HitObjectContainer,
                SpriteEffect = Effects
            };
            
            // Set long note end properties.
            LongNoteEndSprite.Image = GameBase.Skin.Keys[Playfield.Map.Mode].NoteHoldEnds[Index];       
            LongNoteEndSprite.SizeY =  Playfield.LaneSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.SizeY / 2f;    
        }

        /// <summary>
        ///     Gets the correct HitObject texture also based on if we have note snapping and if
        ///     the note is a long note or note.
        ///
        ///     If the user has ColourObjectsBySnapDistance enabled in their skin, we load the one with their
        ///     specified color.
        ///
        ///     If not, we default it to the first beat snap in the list.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHitObjectTexture()
        {
            var skin = GameBase.Skin.Keys[Playfield.Map.Mode];
            
            if (skin.ColorObjectsBySnapDistance)
                return IsLongNote ? skin.NoteHoldHitObjects[Index][SnapIndex] : skin.NoteHitObjects[Index][SnapIndex];
            
            return IsLongNote ? skin.NoteHoldHitObjects[Index][0] : skin.NoteHitObjects[Index][0];
        }
        
        /// <summary>
        ///     Calculates the position from the offset.
        /// </summary>
        /// <returns></returns>
        internal float GetPosFromOffset(float offset)
        {
            var manager = (KeysHitObjectManager) Ruleset.HitObjectManager;

            var speed = GameModeRulesetKeys.IsDownscroll ? -KeysHitObjectManager.ScrollSpeed : KeysHitObjectManager.ScrollSpeed;
            return (float) (manager.HitPositionOffset + (offset - (Ruleset.Screen.Timing.CurrentTime + ConfigManager.GlobalAudioOffset.Value)) * speed);
        }
        
        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        internal void UpdateSpritePositions()
        {           
            // Only update note if it's inside the window
            if ((!GameModeRulesetKeys.IsDownscroll || PositionY + HitObjectSprite.SizeY <= 0) && (GameModeRulesetKeys.IsDownscroll || !(PositionY < GameBase.WindowRectangle.Height))) 
                return;
            
            // Update HitBody
            HitObjectSprite.PosY = PositionY;
;
            // Disregard the rest if it isn't a long note.
            if (!IsLongNote) 
                return;
            
            // It will ignore the rest of the code after this statement if long note size is equal/less than 0
            if (CurrentLongNoteSize <= 0)
            {
                LongNoteBodySprite.Visible = false;
                LongNoteEndSprite.Visible = false;
                return;
            }

            //Update HoldBody Position and Size
            LongNoteBodySprite.SizeY = CurrentLongNoteSize;

            if (GameModeRulesetKeys.IsDownscroll)
            {
                LongNoteBodySprite.PosY = -(float) CurrentLongNoteSize + LongNoteBodyOffset + PositionY;
                LongNoteEndSprite.PosY = PositionY - CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
            }
            else
            {
                LongNoteBodySprite.PosY = PositionY + LongNoteBodyOffset;
                LongNoteEndSprite.PosY = PositionY + CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
            }
        }

        /// <summary>
        ///     When the object iself dies, we want to change it to a dead color.
        /// </summary>
        internal void ChangeSpriteColorToDead()
        {
            if (IsLongNote)
            {
                LongNoteBodySprite.Tint = QuaverColors.DeadNote;
                LongNoteEndSprite.Tint = QuaverColors.DeadNote;
            }

            HitObjectSprite.Tint = QuaverColors.DeadNote;
        }

        /// <summary>
        ///     Fades out the object. Usually used for failure. 
        /// </summary>
        /// <param name="dt"></param>
        internal void FadeOut(double dt)
        {
            HitObjectSprite.FadeOut(dt, 240);

            if (!IsLongNote) 
                return;
            
            LongNoteBodySprite.FadeOut(dt, 240);
            LongNoteEndSprite.FadeOut(dt, 240);
        }
    }
}