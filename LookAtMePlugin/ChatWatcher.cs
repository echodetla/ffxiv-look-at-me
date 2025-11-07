using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Lumina.Text.ReadOnly;

namespace LookAtMePlugin;

public class ChatWatcher : IDisposable
{
    private readonly LookAtMePlugin lookAtMePlugin;
    private readonly IChatGui chat;
    private readonly ITargetManager targetManager;
    private readonly IObjectTable objectTable;
    private readonly IPluginLog log;

    public ChatWatcher(
        LookAtMePlugin lookAtMePlugin, IChatGui chat, ITargetManager targetManager, IObjectTable objectTable,
        IPluginLog log)
    {
        this.lookAtMePlugin = lookAtMePlugin;
        this.chat = chat;
        this.targetManager = targetManager;
        this.objectTable = objectTable;
        this.log = log;
        chat.ChatMessage += ChatOnChatMessage;
    }

    private void ChatOnChatMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        try
        {
            if (isHandled)
                return;

            //todo possibly do this on the main plugin and remove the entire watcher if disabled
            if (!lookAtMePlugin.Configuration.IsEnabled)
                return;

            var enabled = type switch
            {
                XivChatType.Say => lookAtMePlugin.Configuration.MonitorPublicChat,
                XivChatType.Yell => lookAtMePlugin.Configuration.MonitorPublicYellChat,
                XivChatType.Shout => lookAtMePlugin.Configuration.MonitorPublicShoutChat,
                XivChatType.CustomEmote => lookAtMePlugin.Configuration.MonitorPublicChat,
                XivChatType.StandardEmote => lookAtMePlugin.Configuration.MonitorPublicChat,
                XivChatType.Party => lookAtMePlugin.Configuration.MonitorParty,
                XivChatType.FreeCompany => lookAtMePlugin.Configuration.MonitorFc,
                XivChatType.CrossLinkShell1 => lookAtMePlugin.Configuration.MonitorCwl1,
                XivChatType.CrossLinkShell2 => lookAtMePlugin.Configuration.MonitorCwl2,
                XivChatType.CrossLinkShell3 => lookAtMePlugin.Configuration.MonitorCwl3,
                XivChatType.CrossLinkShell4 => lookAtMePlugin.Configuration.MonitorCwl4,
                XivChatType.CrossLinkShell5 => lookAtMePlugin.Configuration.MonitorCwl5,
                XivChatType.CrossLinkShell6 => lookAtMePlugin.Configuration.MonitorCwl6,
                XivChatType.CrossLinkShell7 => lookAtMePlugin.Configuration.MonitorCwl7,
                XivChatType.CrossLinkShell8 => lookAtMePlugin.Configuration.MonitorCwl8,
                _ => false
            };

            if (!enabled)
                return;


            if (lookAtMePlugin.InPvPZone)
                return;

            if (lookAtMePlugin.CachedLocalPlayer != null)
            {
                if (InCombat(lookAtMePlugin.CachedLocalPlayer!))
                {
                    return;
                }
            }


#if DEBUG
            log.Debug("Processing message of type: {type} from sender: {sender}", type, sender.TextValue);
# endif

            var playerPayload = sender.Payloads.FirstOrDefault(x => x.Type == PayloadType.Player) as PlayerPayload;
            if (playerPayload == null)
            {
                //play payload not existing means we can't deduce a player name or more probably its ourself
                return;
            }


            if (playerPayload.PlayerName != string.Empty)
            {
                var senderObject =
                    objectTable.PlayerObjects.FirstOrDefault(x => x.Name.TextValue == playerPayload.PlayerName);
                if (senderObject == null)
                {
                    return;
                }

                targetManager.Target = senderObject;
            }
        }
        catch (Exception ex)
        {
            log.Error($"Error in ChatOnChatMessage: {ex}");
        }
    }

    private static bool InCombat(IGameObject actor) =>
        actor is IPlayerCharacter pc && pc.StatusFlags.HasFlag(StatusFlags.InCombat);


    public void Dispose()
    {
        chat.ChatMessage -= ChatOnChatMessage;
    }
}
