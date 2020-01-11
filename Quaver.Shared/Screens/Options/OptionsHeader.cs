using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options
{
    public class OptionsHeader : Sprite
    {
        /// <summary>
        /// </summary>
        public static int HEIGHT { get; } = 66;

        /// <summary>
        /// </summary>
        private Bindable<OptionsSection> SelectedSection { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TextOptionsMenu { get; set; }

        /// <summary>
        /// </summary>
        private float SidebarWidth { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ActiveSectionText { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="selectedSection"></param>
        /// <param name="width"></param>
        /// <param name="sidebarWidth"></param>
        public OptionsHeader(Bindable<OptionsSection> selectedSection, float width, float sidebarWidth)
        {
            SelectedSection = selectedSection;
            SidebarWidth = sidebarWidth;

            Size = new ScalableVector2(width, HEIGHT);
            Image = AssetLoader.LoadTexture2DFromFile(@"C:\users\swan\desktop\options-header.png");

            CreateTextOptionsMenu();
            CreateActiveSectionText();

            SelectedSection.ValueChanged += OnSelectedSectionChanged;
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
        private void CreateTextOptionsMenu()
        {
            TextOptionsMenu = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "OPTIONS MENU", 26)
            {
                Parent = this,
                Y = -12
            };

            TextOptionsMenu.Y -= TextOptionsMenu.Height;
        }

        /// <summary>
        /// </summary>
        private void CreateActiveSectionText()
        {
            ActiveSectionText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = SidebarWidth + 20
            };

            UpdateActiveSectionText();
        }

        /// <summary>
        /// </summary>
        private void UpdateActiveSectionText() => ScheduleUpdate(() =>
        {
            ActiveSectionText.Text = $"{SelectedSection.Value.Name} Settings".ToUpper();
        });

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedSectionChanged(object sender, BindableValueChangedEventArgs<OptionsSection> e)
            => UpdateActiveSectionText();
    }
}