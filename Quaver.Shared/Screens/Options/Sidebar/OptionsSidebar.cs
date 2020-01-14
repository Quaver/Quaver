using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Options.Sidebar
{
    public class OptionsSidebar : ScrollContainer
    {
        /// <summary>
        /// </summary>
        public static int WIDTH { get; } = 290;

        /// <summary>
        /// </summary>
        private Bindable<OptionsSection> SelectedSection { get; }

        /// <summary>
        /// </summary>
        private List<OptionsSection> Sections { get; }

        /// <summary>
        /// </summary>
        private List<OptionsSidebarSectionButton> SectionButtons { get; set; }

        /// <summary>
        /// </summary>
        private List<OptionsSidebarSubcategoryButton> SubcategoryButtons { get; } = new List<OptionsSidebarSubcategoryButton>();

        /// <summary>
        /// </summary>
        /// <param name="selectedSection"></param>
        /// <param name="sections"></param>
        /// <param name="size"></param>
        public OptionsSidebar(Bindable<OptionsSection> selectedSection, List<OptionsSection> sections, ScalableVector2 size)
            : base(size, size)
        {
            SelectedSection = selectedSection;
            Sections = sections;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 320;

            Scrollbar.Visible = false;
            Image = AssetLoader.LoadTexture2DFromFile(@"C:\users\swan\desktop\options-sidebar.png");

            CreateSidebarButtons();
            AlignAndCreateSubcategoryButtons(false);

            SelectedSection.ValueChanged += OnSelectedSectionChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedSection.ValueChanged -= OnSelectedSectionChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateSidebarButtons()
        {
            SectionButtons = new List<OptionsSidebarSectionButton>();

            foreach (var section in Sections)
            {
                var button = new OptionsSidebarSectionButton(SelectedSection, section, Width);

                SectionButtons.Add(button);
                AddContainedDrawable(button);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="performAnimation"></param>
        private void AlignAndCreateSubcategoryButtons(bool performAnimation)
        {
            var totalHeight = 0f;

            for (var i = 0; i < SectionButtons.Count; i++)
            {
                if (performAnimation)
                    SectionButtons[i].MoveToY((int) totalHeight, Easing.OutQuint, 350);
                else
                    SectionButtons[i].Y = totalHeight;

                totalHeight += OptionsSidebarSectionButton.HEIGHT;

                // Search Result
                if (SelectedSection.Value.Name == string.Empty)
                {
                    SubcategoryButtons.ForEach(x =>
                    {
                        x.Destroy();
                        RemoveContainedDrawable(x);
                    });

                    SubcategoryButtons.Clear();
                }

                // Create subcategory buttons
                if (SelectedSection.Value == SectionButtons[i].Section)
                {
                    SubcategoryButtons.ForEach(x =>
                    {
                        x.Destroy();
                        RemoveContainedDrawable(x);
                    });

                    SubcategoryButtons.Clear();

                    foreach (var subcategory in SelectedSection.Value.Subcategories)
                    {
                        if (string.IsNullOrEmpty(subcategory.Name))
                            continue;

                        var button = new OptionsSidebarSubcategoryButton(SelectedSection, subcategory, Width);
                        button.Y = totalHeight;

                        AddContainedDrawable(button);
                        SubcategoryButtons.Add(button);
                        totalHeight += button.Height;
                    }
                }
            }

            ContentContainer.Height = Math.Max(Height, totalHeight);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedSectionChanged(object sender, BindableValueChangedEventArgs<OptionsSection> e)
        {
            AlignAndCreateSubcategoryButtons(true);

            ContentContainer.Animations.Clear();
            ContentContainer.Y = 0;
            TargetY = 0;
            PreviousTargetY = 0;
        }
    }
}