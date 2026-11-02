using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects.Twitch;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.SongRequests;

public sealed class SongRequestRightClickOptions : V2Dropdown<SongRequestMenuAction>
{
    private SongRequest Request { get; }
    private Action<SongRequest> PlayRequest { get; }
    private Action<SongRequest> DeleteRequest { get; }

    public SongRequestRightClickOptions(SongRequest request, float width, SkinV2DropdownConfig config, Container overlayHost,
        Action<SongRequest> playRequest, Action<SongRequest> deleteRequest)
        : base(width, GetOptions(request), FontManager.GetWobbleFont(Fonts.InterBold), config, overlayHost)
    {
        Request = request;
        PlayRequest = playRequest;
        DeleteRequest = deleteRequest;
        OptionSelected += OnOptionSelected;
    }

    private static IReadOnlyList<DropdownEntry<SongRequestMenuAction>> GetOptions(SongRequest request)
    {
        var options = new List<DropdownEntry<SongRequestMenuAction>>
        {
            new DropdownOption<SongRequestMenuAction>(SongRequestMenuAction.Play, LocalizationManager.Get("Screen_Selection_Play"), Color.White)
        };

        if (!string.IsNullOrEmpty(request.TwitchUsername))
            options.Add(new DropdownOption<SongRequestMenuAction>(SongRequestMenuAction.RequesterProfile, LocalizationManager.Get("Screen_Hub_RequesterProfile"), ColorHelper.HexToColor("#0787E3")));

        if (request.MapsetId > 0 && ((MapGame)request.Game == MapGame.Quaver || (MapGame)request.Game == MapGame.Osu))
            options.Add(new DropdownOption<SongRequestMenuAction>(SongRequestMenuAction.OnlineListing, LocalizationManager.Get("Screen_Editor_ViewOnlineListing"), ColorHelper.HexToColor("#9B51E0")));

        options.Add(new DropdownOption<SongRequestMenuAction>(SongRequestMenuAction.Delete, LocalizationManager.Get("Screen_Editor_Delete"), ColorHelper.HexToColor("#FF6868")));
        return options;
    }

    private void OnOptionSelected(object sender, DropdownOptionEventArgs<SongRequestMenuAction> e)
    {
        switch (e.Option.Value)
        {
            case SongRequestMenuAction.Play:
                PlayRequest(Request);
                break;
            case SongRequestMenuAction.RequesterProfile:
                BrowserHelper.OpenURL($"https://twitch.tv/{Uri.EscapeDataString(Request.TwitchUsername)}");
                break;
            case SongRequestMenuAction.OnlineListing:
                var url = (MapGame)Request.Game == MapGame.Osu
                    ? $"https://osu.ppy.sh/beatmapsets/{Request.MapsetId}"
                    : $"https://quavergame.com/mapsets/{Request.MapsetId}/maps/{Request.MapId}";
                BrowserHelper.OpenURL(url);
                break;
            case SongRequestMenuAction.Delete:
                DeleteRequest(Request);
                break;
        }
    }

    public override void Destroy()
    {
        OptionSelected -= OnOptionSelected;
        base.Destroy();
    }
}

public enum SongRequestMenuAction
{
    Play,
    RequesterProfile,
    OnlineListing,
    Delete
}
