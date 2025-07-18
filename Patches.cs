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
#endif

using HarmonyLib;
using MelonLoader;

namespace Timestamp
{
    [HarmonyPatch(typeof(Player), "SleepStart")]
    public class SleepStartPatch
    {
        static void Prefix() => TimestampTexts.DaySent.Clear();
    }

    [HarmonyPatch(typeof(MessagesApp), "SetCurrentConversation")]
    public class SetCurrentConversationPatch
    {
        static void Prefix(MSGConversation conversation)
        {
            string contact   = conversation?.contactName;
            bool showDay     = TimestampTexts.ShowDayNamePref.Value;
            bool alreadySent = TimestampTexts.DaySent.TryGetValue(contact, out _);

            if (!showDay || alreadySent || conversation is null || contact is null)
                return;

            string day = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
            if (TimestampTexts.ShowDayNumPref.Value)
                day += ", Day " + NetworkSingleton<TimeManager>.Instance.ElapsedDays;

            TimestampTexts.DaySent  .Add(contact);
            TimestampTexts.BlockMove.Add(contact);

            TimestampTexts.SilenceMessage();
            conversation.SendMessage(new Message(day, Message.ESenderType.Other), notify:false, network:false);
        }
    }

    [HarmonyPatch(typeof(MSGConversation), "SendMessage")]
    public class SendMessagePatch
    {
        static void Prefix(Message message, bool notify = true, bool network = true)
        {
            message.text = TimestampTexts.SetTimestamp(message);
        }
    }

    [HarmonyPatch(typeof(MSGConversation), "SendMessageChain")]
    public class SendMessageChainPatch
    {
        static void Prefix(MessageChain messages, float initialDelay = 0f, bool notify = true, bool network = true)
        {
            for (int i = 0; i < messages.Messages.Count; i++)
                messages.Messages[i] = TimestampTexts.SetTimestamp(messages.Messages[i], Message.ESenderType.Other);
        }
    }

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
}