#if   Il2Cpp
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.UI.Phone.Messages;

#elif Mono
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using ScheduleOne.PlayerScripts;
using ScheduleOne.GameTime;
using ScheduleOne.UI.Phone.Messages;
using System.Reflection;
using UnityEngine.UI;
#endif

using HarmonyLib;
using MelonLoader;

namespace Timestamp
{
    //********************
    //Player Patches
    //********************

    //Resets the list of conversations where the day has already been shown.
    //Runs nightly just before the player sleeps.
    [HarmonyPatch(typeof(Player), "SleepStart")]
    public class SleepStartPatch
    {
        static void Prefix()
        {
            TimestampTexts.DaySent.Clear();
        }
    }
    
    //Loops through the list of active conversations (i.e. those that have been rendered this session) and sends the day name in each.
    //CreateDayName() is what checks the player preference for sending the message, and returns null if disabled.
    //It's a bit inelegant but I did this way to keep the checking centralized in a shared function. Still, I don't love it.
    //Runs nightly just after the player sleeps ('morningly' I guess).
    [HarmonyPatch(typeof(Player), "SleepEnd")]
    public class SleepEndPatch
    {
        static void Postfix()
        {
            foreach (MSGConversation convo in MessagesApp.ActiveConversations)
                if (!(convo is null) && convo.messageHistory.Count > 0)
                {
                    Message msg = TimestampTexts.CreateDayName(convo.contactName);
                    if (!(msg is null))
                        convo.SendMessage(msg, notify: false, network: false);
                }
        }
    }

    //********************
    //MSGConversation Patches
    //********************

    //Grabs messages as they are sent and sends them to be timestamped.
    //MSGConversation.SendMessage() is generally called when the player sends a text to NPCs, and what I call for sending custom texts.
    [HarmonyPatch(typeof(MSGConversation), "SendMessage")]
    public class SendMessagePatch
    {
        static void Prefix(Message message, bool notify = true, bool network = true)
        {
            if (message is null) return;
            message.text = TimestampTexts.SetTimestamp(message);
        }
    }

    //Grabs messages as they are sent and sends them to be timestamped.
    //MSGConversation.SendMessageChain() is generally called when an NPC sends a text to the player.
    [HarmonyPatch(typeof(MSGConversation), "SendMessageChain")]
    public class SendMessageChainPatch
    {
        static void Prefix(MessageChain messages, float initialDelay = 0f, bool notify = true, bool network = true)
        {
            for (int i = 0; i < messages.Messages.Count; i++)
                messages.Messages[i] = TimestampTexts.SetTimestamp(messages.Messages[i], Message.ESenderType.Other);
        }
    }

    //MSGConversation.MoveToTop() puts a conversation at the top of the list whenever it receives a text.
    //This patch looks for a BlockMove flag that is set when CreateDayName() is sent in that conversation, and prevents the function from executing.
    //(I.E. we don't want our "Saturday, Day 28" label to count as a real text and move the conversation to the top).
    [HarmonyPatch(typeof(MSGConversation), "MoveToTop")]
    public class MoveToTopPatch
    {
        static bool Prefix(MSGConversation __instance)
        {
            string name = __instance?.sender?.fullName ?? null;
            if (string.IsNullOrEmpty(name) || !TimestampTexts.BlockMove.TryGetValue(name, out _)) 
                return true;

            TimestampTexts.BlockMove.Remove(name);
            return false;
        }
    }

    //MSGConversation.RefreshPreviewText() updates the conversation list view to show the most recent text in each.
    //This patch checks if that's the day name name, and if so shows the next one instead.
    //Since it's only a single check, it will show the day the name if two most recent texts are such (e.g. no actual texts in that convo yesterday).
    //That's a more minor edge case though so I've left it as it is for now.
    //The entryPreviewText object is protected, so Mono requires reflection to access it.
    [HarmonyPatch(typeof(MSGConversation), "RefreshPreviewText")]
    public class MSGConversationRefreshPreviewTextPatch
    {
        static void Postfix(MSGConversation __instance)
        {
            if (__instance is null || __instance.bubbles == null || __instance.bubbles.Count == 0) return;
            string day = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
            int count  = __instance.bubbles.Count;
#if Il2Cpp
            if (count > 1 && __instance.entryPreviewText.text.StartsWith(day))
                __instance.entryPreviewText.text = __instance.bubbles[count - 2].text;
#elif Mono
            FieldInfo field = AccessTools.Field(typeof(MSGConversation), "entryPreviewText");

            if (field != null && count > 1 && field.GetValue(__instance) is Text preview && preview.text.StartsWith(day))
                    preview.text = __instance.bubbles[count - 2].text;
#endif
        }
    }
}