#if   Il2Cpp
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Messaging;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.UI.Phone.Messages;

#elif Mono
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using ScheduleOne.GameTime;
using ScheduleOne.UI.Phone.Messages;

#endif
using MelonLoader;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Timestamp
{
    public class TimestampTexts : MelonMod
    {
        //DaySent tracks conversations that already have today's day name sent.
        //BlockMove tracks conversations that *just* had the day name sent, to prevent moving to top of convo list.
        public static HashSet<string> DaySent   = new HashSet<string>();
        public static HashSet<string> BlockMove = new HashSet<string>();

        public static MelonPreferences_Category    DisplayPrefs     { get; private set; }
        public static MelonPreferences_Category    SentTextPrefs    { get; private set; }
        public static MelonPreferences_Category    RcvdTextPrefs    { get; private set; }
        
        //DisplayPrefs:
        public static MelonPreferences_Entry<bool> ShowSentPref     { get; private set; }
        public static MelonPreferences_Entry<bool> ShowRcvdPref     { get; private set; }
        public static MelonPreferences_Entry<bool> ShowDayNamePref  { get; private set; }
        public static MelonPreferences_Entry<bool> ShowDayNumPref   { get; private set; }
        public static MelonPreferences_Entry<bool> ShowAmPmPref     { get; private set; }
        
        //SentTextPrefs:
        public static MelonPreferences_Entry<bool> SentItalicsPref  { get; private set; }
        public static MelonPreferences_Entry<bool> SentIsSizedPref  { get; private set; }
        public static MelonPreferences_Entry<bool> SentIsColorPref  { get; private set; }
        public static MelonPreferences_Entry<bool> SentBracketPref  { get; private set; }
        public static MelonPreferences_Entry<bool> SentPrefixPref   { get; private set; }
        public static MelonPreferences_Entry<bool> SentNewlinePref  { get; private set; }
        public static MelonPreferences_Entry<int>  SentSizePref     { get; private set; }
        
        //RcvdTextPrefs:
        public static MelonPreferences_Entry<bool> RcvdItalicsPref  { get; private set; }
        public static MelonPreferences_Entry<bool> RcvdIsSizedPref  { get; private set; }
        public static MelonPreferences_Entry<bool> RcvdIsColorPref  { get; private set; }
        public static MelonPreferences_Entry<bool> RcvdBracketPref  { get; private set; }
        public static MelonPreferences_Entry<bool> RcvdPrefixPref   { get; private set; }
        public static MelonPreferences_Entry<bool> RcvdNewlinePref  { get; private set; }
        public static MelonPreferences_Entry<int>  RcvdSizePref     { get; private set; }

        //The build split here is only for PhoneModManager support: Il2Cpp version has a color chooser, Mono does not.
        //The split isn't strictly required but I did it to fully take advantage of the picker in main build and use a standard hex string in alternate,
        //without needing any differences in the primary logic code. 
#if Il2Cpp
        public static MelonPreferences_Entry<Color> SentColorPref   { get; private set; }
        public static MelonPreferences_Entry<Color> RcvdColorPref   { get; private set; }

        private const string Filepath         = "UserData/TimestampIl2Cpp.cfg";
        public static readonly Color Grey     = new Color(0.4f, 0.4f, 0.4f);
        public static readonly string Default = $"[ {Mathf.Round(Grey.r * 255)}.0, {Mathf.Round(Grey.g * 255)}.0, {Mathf.Round(Grey.b * 2550)}.0, 255.0, ]";

#elif Mono
        public static MelonPreferences_Entry<string> SentColorPref  { get; private set; }
        public static MelonPreferences_Entry<string> RcvdColorPref  { get; private set; }

        private const string Filepath = "UserData/TimestampMono.cfg";
        public const string  Grey     = "666666";
        public const string  Default  = Grey;

#endif
        public override void OnInitializeMelon() => InitializeSettings();

        public static void InitializeSettings()
        {
            DisplayPrefs  = MelonPreferences.CreateCategory("TimestampTexts_Pref", "Display Options");
            SentTextPrefs = MelonPreferences.CreateCategory("TimestampTexts_Sent", "Sent Texts");
            RcvdTextPrefs = MelonPreferences.CreateCategory("TimestampTexts_Rcvd", "Received Texts");

            DisplayPrefs .SetFilePath(Filepath);
            SentTextPrefs.SetFilePath(Filepath);
            RcvdTextPrefs.SetFilePath(Filepath);

            ShowSentPref    = DisplayPrefs.CreateEntry("Pref1ShowSent", default_value: true,  "Timestamp Sent Messages?");
            ShowRcvdPref    = DisplayPrefs.CreateEntry("Pref2ShowRcvd", default_value: true,  "Timestamp Received Messages?");
            ShowAmPmPref    = DisplayPrefs.CreateEntry("Pref3ShowAmPm", default_value: true,  "Include AM / PM Label?");
            ShowDayNamePref = DisplayPrefs.CreateEntry("Pref4ShowDay",  default_value: true,  "Show Day Name?");
            ShowDayNumPref  = DisplayPrefs.CreateEntry("Pref5ShowDate", default_value: true,  "Include Day Number?");

            SentIsSizedPref = SentTextPrefs.CreateEntry("Sent1IsSized", default_value: true,  "Change Text Size?");
            SentSizePref    = SentTextPrefs.CreateEntry("Sent2Size",    default_value: 28,    "Timestamp Text Size:");
            SentIsColorPref = SentTextPrefs.CreateEntry("Sent3IsColor", default_value: true,  "Use Color?");
            SentColorPref   = SentTextPrefs.CreateEntry("Sent4Color",   default_value: Grey,  "Timestamp Text Color:");
            SentItalicsPref = SentTextPrefs.CreateEntry("Sent5Italics", default_value: true,  "Use Italics?");
            SentBracketPref = SentTextPrefs.CreateEntry("Sent6Bracket", default_value: false, "Put in Brackets?");
            SentPrefixPref  = SentTextPrefs.CreateEntry("Sent7Prefix",  default_value: false, "Time before Message? (False for after)");
            SentNewlinePref = SentTextPrefs.CreateEntry("Sent8Newline", default_value: true,  "Timestamp on Separate Line?");

            RcvdIsSizedPref = RcvdTextPrefs.CreateEntry("Rcvd1IsSized", default_value: true,  "Change Text Size?");
            RcvdSizePref    = RcvdTextPrefs.CreateEntry("Rcvd2Size",    default_value: 28,    "Timestamp Text Size:");
            RcvdIsColorPref = RcvdTextPrefs.CreateEntry("Rcvd3IsColor", default_value: true,  "Use Color?");
            RcvdColorPref   = RcvdTextPrefs.CreateEntry("Rcvd4Color",   default_value: Grey,  "Timestamp Text Color:");
            RcvdItalicsPref = RcvdTextPrefs.CreateEntry("Rcvd5Italics", default_value: true,  "Use Italics?");
            RcvdBracketPref = RcvdTextPrefs.CreateEntry("Rcvd6Bracket", default_value: false, "Put in Brackets?");
            RcvdPrefixPref  = RcvdTextPrefs.CreateEntry("Rcvd7Prefix",  default_value: false, "Time before Message? (False for after)");
            RcvdNewlinePref = RcvdTextPrefs.CreateEntry("Rcvd8Newline", default_value: true,  "Timestamp on Separate Line?");

            ShowSentPref.Comment    = "Show Timestamp on sent messages when True (default true)";
            ShowRcvdPref.Comment    = "Show Timestamp on received messages when True (default true)";
            ShowAmPmPref.Comment    = "Show AM or PM after Timestamp when True (default true)";
            ShowDayNamePref.Comment = "Show name of each day e.g. \"Tuesday\" as the first text of each conversation (default true)";
            ShowDayNumPref.Comment  = "Include day number (running count of days in current save) (default true)";

            SentIsSizedPref.Comment = "Change size of sent Timestamp (set False to use default text message size) (default true)";
            SentSizePref.Comment    = "Custom font size for sent Timestamp, adjust to preference (default 28)";
            SentIsColorPref.Comment = "Change color of sent Timestamp (set False to use default text message color) (default true)";
            SentColorPref.Comment   = $"Custom color for sent Tiemstamp, adjust to preference (default {Default})";
            SentItalicsPref.Comment = "Show sent Timestamp in Italics (default true)";
            SentBracketPref.Comment = "Show sent Timestamp in [brackets] (default false)";
            SentPrefixPref.Comment  = "Show Timestamp before sent message if True, after message if False (default false)";
            SentNewlinePref.Comment = "Show Timestamp on separate line from sent message if True, inline if False (default true)";

            RcvdIsSizedPref.Comment = "Change size of received Timestamp (set False to use default text message size) (default true)";
            RcvdSizePref.Comment    = "Custom font size for received Timestamp, adjust to preference (default 28)";
            RcvdIsColorPref.Comment = "Change color of received Timestamp (set False to use default text message color) (default true)";
            RcvdColorPref.Comment   = $"Custom color for received Tiemstamp, adjust to preference (default {Default})";
            RcvdItalicsPref.Comment = "Show received Timestamp in Italics (default true)";
            RcvdBracketPref.Comment = "Show received Timestamp in [brackets] (default false)";
            RcvdPrefixPref.Comment  = "Show Timestamp before received message if True, after message if False (default false)";
            RcvdNewlinePref.Comment = "Show Timestamp on separate line from received message if True, inline if False (default true)";
        }

        public static string SetTimestamp(Message message) => SetTimestamp(message.text, message.sender);

        public static string SetTimestamp(string message, Message.ESenderType sender)
        {
            bool   sent = sender == Message.ESenderType.Player;
            bool   show = sent ? ShowSentPref.Value : ShowRcvdPref.Value;
            string time = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime, ShowAmPmPref.Value);
            string day  = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();

            if (!show || message.Contains(day) || message.Contains(time))
                return message;

            bool prefix  = sent ? SentPrefixPref.Value  : RcvdPrefixPref.Value;
            bool newline = sent ? SentNewlinePref.Value : RcvdNewlinePref.Value;
            bool italics = sent ? SentItalicsPref.Value : RcvdItalicsPref.Value;
            bool isSized = sent ? SentIsSizedPref.Value : RcvdIsSizedPref.Value;
            bool isColor = sent ? SentIsColorPref.Value : RcvdIsColorPref.Value;
            bool bracket = sent ? SentBracketPref.Value : RcvdBracketPref.Value;
            int size     = sent ? SentSizePref.Value    : RcvdSizePref.Value;

#if Il2Cpp
            Color col = sent ? SentColorPref.Value : RcvdColorPref.Value;
            string color = "#" + ColorUtility.ToHtmlStringRGB(col);
#elif Mono
            string color = "#" + (sent ? SentColorPref.Value : RcvdColorPref.Value);
#endif

            if (bracket) time = $"[{time}]";
            if (italics) time = $"<i>{time}</i>";
            if (isSized) time = $"<size={size}>{time}</size>";
            if (isColor) time = $"<color={color}>{time}</color>";

            string sep = newline ? "\n" : " ";
            message = prefix ? time + sep + message : message + sep + time;

            return message;
        }

        public static Message CreateDayName(string contact)
        {
            bool showDay     = ShowDayNamePref.Value;
            bool alreadySent = DaySent.TryGetValue(contact, out _);

            if (!showDay || alreadySent)
                return null;

            string day = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
            if (ShowDayNumPref.Value)
                day += ", Day " + NetworkSingleton<TimeManager>.Instance.ElapsedDays;

            DaySent  .Add(contact);
            BlockMove.Add(contact);

            SilenceMessage();
            return new Message(day, Message.ESenderType.Other);
        }

        //I couldn't find a good direct way to prevent the ping sound on received messages, so this is a small hack.
        //It simply mutes the AudioController linked to the messages app for 0.1 seconds when a message we want to silence is coming.
        //There may be a more proper way to achieve this but this seems to work without side effect.
        public static void SilenceMessage()
        {
            AudioSourceController sound = PlayerSingleton<MessagesApp>.Instance?.MessageReceivedSound ?? null;
            if (sound is null) return;

            float volume = sound.VolumeMultiplier;
            sound.VolumeMultiplier = 0;
            MelonCoroutines.Start(RestoreSoundLater(sound, volume));
        }

        private static IEnumerator RestoreSoundLater(AudioSourceController sound, float volume)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            sound.VolumeMultiplier = volume;
        }
    }
}