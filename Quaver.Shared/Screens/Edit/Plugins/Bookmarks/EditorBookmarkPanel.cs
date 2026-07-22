using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Plugins.Timing;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Edit.Plugins.Bookmarks;

public class EditorBookmarkPanel : SpriteImGui, IEditorPlugin, IColoredImGuiTitle
{
    private EditScreen Screen { get; }

    public string Name { get; } = "Bookmark Editor";

    public string Author { get; } = "The Quaver Team";

    public string Description { get; set; } = "";

    public bool IsBuiltIn { get; set; } = true;

    public string Directory { get; set; }

    public bool IsWorkshop { get; set; }

    public Dictionary<string, EditorPluginStorageValue> Storage { get; set; } = new();

    public bool IsActive { get; set; }

    public bool IsWindowHovered { get; private set; }

    private List<BookmarkInfo> SelectedBookmarks { get; } = new();

    private List<BookmarkInfo> Clipboard { get; } = new();

    private int? NeedsToScrollToFirstSelectedBookmark { get; set; }

    private int? NeedsToScrollToLastSelectedBookmark { get; set; }

    public Color TitleColor => PaulToulColorGenerator.ColorScheme[18];

    public EditorBookmarkPanel(EditScreen screen)
        : base(false, EditorImGuiOptions.GetOptions(), screen.ImGuiScale)
    {
        Screen = screen;
        Initialize();
    }

    public void Initialize()
    {
        SelectedBookmarks.Clear();

        var bookmark = Screen.WorkingMap.GetBookmarkAt((int)Screen.Track.Time);
        if (bookmark == null)
            return;

        SelectedBookmarks.Add(bookmark);

        if (bookmark != Screen.WorkingMap.Bookmarks.First())
            NeedsToScrollToFirstSelectedBookmark = Screen.WorkingMap.Bookmarks.IndexOf(bookmark);
    }

    public void OnStorageLoaded()
    {
    }

    public void OnStorageSave()
    {
    }

    protected override void RenderImguiLayout()
    {
        SelectedBookmarks.RemoveAll(x => !Screen.WorkingMap.Bookmarks.Contains(x));

        ImGui.SetNextWindowSizeConstraints(new Vector2(450, 0), new Vector2(600, float.MaxValue));
        ImGui.PushFont(Options.Fonts.First().Context);
        ((IColoredImGuiTitle)this).ImGuiPushTitleColors();
        ImGui.Begin(Name);

        DrawHeaderText();
        ImGui.Dummy(new Vector2(0, 10));

        DrawSelectCurrentBookmarkButton();
        ImGui.Dummy(new Vector2(0, 10));

        DrawAddButton();
        ImGui.SameLine();
        DrawRemoveButton();

        ImGui.Dummy(new Vector2(0, 10));

        if (SelectedBookmarks.Count <= 1)
            DrawTimeTextbox();
        else
            DrawMoveOffsetByTextbox();

        ImGui.Dummy(new Vector2(0, 10));
        DrawNoteTextbox();

        var isHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();

        ImGui.Dummy(new Vector2(0, 10));
        DrawSelectedCountLabel();

        ImGui.Dummy(new Vector2(0, 10));
        DrawTable();

        IsWindowHovered = IsWindowHovered || isHovered;
        ImGui.End();
        ((IColoredImGuiTitle)this).ImGuiPopTitleColors();
    }

    private static void DrawHeaderText()
    {
        ImGui.TextWrapped("Bookmarks mark important positions in the map and can include a note and a custom color.");
        ImGui.Dummy(new Vector2(0, 5));
        ImGui.TextWrapped("Click a bookmark to select it, or click it again to seek to its position in time.");
    }

    private void DrawAddButton()
    {
        if (!ImGui.Button("Add"))
            return;

        var bookmark = new BookmarkInfo
        {
            StartTime = (int)Math.Round(Screen.Track.Time, MidpointRounding.AwayFromZero),
            Note = ""
        };

        Screen.ActionManager.AddBookmark(bookmark);

        SelectedBookmarks.Clear();
        SelectedBookmarks.Add(bookmark);
        NeedsToScrollToFirstSelectedBookmark = Screen.WorkingMap.Bookmarks.IndexOf(bookmark);
    }

    private void DrawRemoveButton()
    {
        if (!ImGui.Button("Remove") || SelectedBookmarks.Count == 0)
            return;

        var lastBookmark = SelectedBookmarks.OrderBy(x => x.StartTime).Last();
        Screen.ActionManager.RemoveBookmarkBatch(new List<BookmarkInfo>(SelectedBookmarks));

        SelectedBookmarks.Clear();

        var newBookmark = Screen.WorkingMap.Bookmarks.FindLast(x => x.StartTime <= lastBookmark.StartTime);
        if (newBookmark == null)
            return;

        SelectedBookmarks.Add(newBookmark);
        NeedsToScrollToFirstSelectedBookmark = Screen.WorkingMap.Bookmarks.IndexOf(newBookmark);
    }

    private void DrawSelectCurrentBookmarkButton()
    {
        if (ImGui.Button("Select current bookmark"))
        {
            var currentBookmark = Screen.WorkingMap.GetBookmarkAt((int)Screen.Track.Time);
            if (currentBookmark != null)
            {
                NeedsToScrollToLastSelectedBookmark = Screen.WorkingMap.Bookmarks.IndexOf(currentBookmark);
                SelectBookmark(currentBookmark, KeyboardManager.IsCtrlDown(), KeyboardManager.IsShiftDown());
            }
        }

        if (!ImGui.IsItemHovered())
            return;

        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25);
        ImGui.Text("Selects the bookmark at the current editor timestamp. Ctrl adds it to the selection, and Shift selects the range to it.");
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    private void SelectBookmark(BookmarkInfo bookmark, bool control, bool shift)
    {
        if (control)
        {
            if (SelectedBookmarks.Contains(bookmark))
                SelectedBookmarks.Remove(bookmark);
            else
                SelectedBookmarks.Add(bookmark);

            return;
        }

        if (shift && SelectedBookmarks.Count > 0)
        {
            var min = Math.Min(bookmark.StartTime, SelectedBookmarks.Min(x => x.StartTime));
            var max = Math.Max(bookmark.StartTime, SelectedBookmarks.Max(x => x.StartTime));

            SelectedBookmarks.AddRange(Screen.WorkingMap.Bookmarks
                .Where(x => x.StartTime >= min && x.StartTime <= max));

            var distinct = SelectedBookmarks.Distinct().OrderBy(x => x.StartTime).ToList();
            SelectedBookmarks.Clear();
            SelectedBookmarks.AddRange(distinct);
            return;
        }

        if (SelectedBookmarks.Contains(bookmark))
            Screen.Track.Seek(bookmark.StartTime);

        SelectedBookmarks.Clear();
        SelectedBookmarks.Add(bookmark);
    }

    private void DrawTimeTextbox()
    {
        var time = SelectedBookmarks.Count == 1 ? SelectedBookmarks[0].StartTime : 0;

        ImGui.TextWrapped("Time");

        if (ImGui.InputInt("##bookmarkTime", ref time, 1, 10,
                ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll) &&
            SelectedBookmarks.Count == 1)
        {
            var bookmark = SelectedBookmarks[0];
            Screen.ActionManager.ChangeBookmarkBatchOffset(new List<BookmarkInfo> { bookmark },
                time - bookmark.StartTime);
            NeedsToScrollToFirstSelectedBookmark = Screen.WorkingMap.Bookmarks.IndexOf(bookmark);
        }
    }

    private void DrawMoveOffsetByTextbox()
    {
        var offset = 0;

        ImGui.TextWrapped("Move Times By");

        if (ImGui.InputInt("##bookmarkOffset", ref offset, 1, 10,
                ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            Screen.ActionManager.ChangeBookmarkBatchOffset(SelectedBookmarks, offset);
    }

    private void DrawNoteTextbox()
    {
        var note = SelectedBookmarks.Count == 1 ? SelectedBookmarks[0].Note ?? "" : "";

        ImGui.TextWrapped("Note");
        ImGui.BeginDisabled(SelectedBookmarks.Count != 1);

        if (ImGui.InputText("##bookmarkNote", ref note, 1024,
                ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll) &&
            SelectedBookmarks.Count == 1)
            Screen.ActionManager.EditBookmark(SelectedBookmarks[0], note);

        ImGui.EndDisabled();
    }

    private void ShowColorDialog()
    {
        if (SelectedBookmarks.Count == 0)
            return;

        DialogManager.Show(new EditorChangeBookmarkColorDialog(new List<BookmarkInfo>(SelectedBookmarks),
            Screen.ActionManager));
    }

    private void DrawSelectedCountLabel()
    {
        var count = SelectedBookmarks.Count;
        ImGui.Text(count > 1 ? $"{count} bookmarks selected" : "");
    }

    private void DrawTable()
    {
        if (!ImGui.BeginTable("##BookmarkTable", 3,
                ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchSame))
        {
            IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
            return;
        }

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Time");
        ImGui.TableSetupColumn("Note");
        ImGui.TableSetupColumn("Color");
        ImGui.TableHeadersRow();

        if (Screen.WorkingMap.Bookmarks.Count == 0)
        {
            NeedsToScrollToFirstSelectedBookmark = null;
            NeedsToScrollToLastSelectedBookmark = null;
        }

        for (var i = 0; i < Screen.WorkingMap.Bookmarks.Count; i++)
        {
            var bookmark = Screen.WorkingMap.Bookmarks[i];
            var isSelected = SelectedBookmarks.Contains(bookmark);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.PushID(i);

            ScrollToSelection(bookmark);

            if (!isSelected)
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(100, 100, 100, 0));

            if (ImGui.Button($@"{TimeSpan.FromMilliseconds(bookmark.StartTime):mm\:ss\.fff}"))
                SelectBookmark(bookmark, KeyboardManager.IsCtrlDown(), KeyboardManager.IsShiftDown());

            if (!isSelected)
                ImGui.PopStyleColor();

            ImGui.TableNextColumn();
            ImGui.TextWrapped(bookmark.Note ?? "");

            ImGui.TableNextColumn();
            var bookmarkColor = ColorHelper.ToXnaColor(bookmark.GetColor());
            var colorVector = new Vector4(bookmarkColor.R, bookmarkColor.G, bookmarkColor.B, byte.MaxValue) /
                              byte.MaxValue;
            const ImGuiColorEditFlags colorOptions = ImGuiColorEditFlags.Float | ImGuiColorEditFlags.NoInputs |
                                                     ImGuiColorEditFlags.NoPicker;

            if (ImGui.ColorButton("##rowColor", colorVector, colorOptions))
            {
                if (!SelectedBookmarks.Contains(bookmark))
                {
                    SelectedBookmarks.Clear();
                    SelectedBookmarks.Add(bookmark);
                }

                ShowColorDialog();
            }

            ImGui.PopID();
        }

        IsWindowHovered = ImGui.IsWindowHovered() || ImGui.IsAnyItemFocused();
        HandleInput();
        ImGui.EndTable();
    }

    private void ScrollToSelection(BookmarkInfo bookmark)
    {
        if (SelectedBookmarks.Count == 0)
            return;

        if (NeedsToScrollToLastSelectedBookmark.HasValue && SelectedBookmarks[^1] == bookmark &&
            !NeedsToScrollToFirstSelectedBookmark.HasValue)
        {
            ImGui.SetScrollHereY(-0.025f);
            NeedsToScrollToLastSelectedBookmark = null;
        }
        else if (NeedsToScrollToFirstSelectedBookmark.HasValue && SelectedBookmarks[0] == bookmark)
        {
            ImGui.SetScrollHereY(-0.025f);
            NeedsToScrollToFirstSelectedBookmark = null;
        }
    }

    private void HandleInput()
    {
        if (!IsWindowHovered || ImGui.GetIO().WantTextInput)
            return;

        if (KeyboardManager.IsCtrlDown())
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
            {
                SelectedBookmarks.Clear();
                SelectedBookmarks.AddRange(Screen.WorkingMap.Bookmarks);
            }
            else if (KeyboardManager.IsUniqueKeyPress(Keys.D))
                SelectedBookmarks.Clear();
            else if (KeyboardManager.IsUniqueKeyPress(Keys.X))
                CutClipboard();
            else if (KeyboardManager.IsUniqueKeyPress(Keys.C))
                CopyToClipboard();
            else if (KeyboardManager.IsUniqueKeyPress(Keys.V))
                PasteClipboard();
        }

        if (KeyboardManager.IsUniqueKeyPress(Keys.Delete) && SelectedBookmarks.Count != 0)
        {
            Screen.ActionManager.RemoveBookmarkBatch(new List<BookmarkInfo>(SelectedBookmarks));
            SelectedBookmarks.Clear();
        }
    }

    private void CutClipboard()
    {
        CopyToClipboard();
        Screen.ActionManager.RemoveBookmarkBatch(new List<BookmarkInfo>(SelectedBookmarks));
        SelectedBookmarks.Clear();
    }

    private void CopyToClipboard()
    {
        Clipboard.Clear();
        Clipboard.AddRange(SelectedBookmarks.Select(CloneBookmark));
    }

    private void PasteClipboard()
    {
        if (Clipboard.Count == 0)
            return;

        var firstTime = Clipboard.Min(x => x.StartTime);
        var difference = (int)Math.Round(Screen.Track.Time - firstTime, MidpointRounding.AwayFromZero);
        var pastedBookmarks = Clipboard.Select(x =>
        {
            var clone = CloneBookmark(x);
            clone.StartTime += difference;
            return clone;
        }).OrderBy(x => x.StartTime).ToList();

        Screen.ActionManager.AddBookmarkBatch(pastedBookmarks);
        SelectedBookmarks.Clear();
        SelectedBookmarks.AddRange(pastedBookmarks);
        NeedsToScrollToFirstSelectedBookmark = Screen.WorkingMap.Bookmarks.IndexOf(pastedBookmarks[0]);
    }

    private static BookmarkInfo CloneBookmark(BookmarkInfo bookmark) => new()
    {
        StartTime = bookmark.StartTime,
        Note = bookmark.Note,
        ColorRgb = bookmark.ColorRgb
    };
}
