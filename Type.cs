﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace mkbot
{
    public class User
    {
        /// <summary>
        /// ファイルからの復元用
        /// </summary>
        /// <param name="username">@名</param>
        /// <param name="host">ホスト</param>
        public User(string username, string host, int love,List<DateTime> changetimes)
        {
            this.username = username;
            Host = host;
            Love = love;
            LoveChangedTime = changetimes;
        }
        /// <summary>
        /// ユーザー登録と初期化
        /// </summary>
        /// <param name="username">@名</param>
        /// <param name="host">ホスト</param>
        public User(string username, string host)
        {
            Register(username, host);
        }
        /// <summary>
        /// 最大1000(100)で下1桁が小数点みたいな感じで
        /// </summary>
        const int LoveMax = 1000;
        public const int LoveUnit0 = -10;
        public const int LoveUnit1 = 10;
        public const int LoveUnit2 = 5;
        public readonly List<int> LoveUnits = (new (new int[3] {-10,10,5 }));
        const int LoveThreshold =  750;
        const int HateThreshold = -100;
        public enum LoveLevel
        {
            Hate,
            Normal,
            Love
        }
        public LoveLevel GetLoveLevel()
        {
            return Love switch
            {
                >LoveThreshold => LoveLevel.Love,
                <HateThreshold => LoveLevel.Hate,
                _ => LoveLevel.Normal
            };
        }
        public void Register(string username ,string host) 
        { 
            this.username = username;
            Host = host;
            Love = 0;
            LoveChangedTime = new List<DateTime>();
        }
        public void CalcLove(int Unit )
        {
            if (LoveMax <= Love || Unit == 0)
            {
                Console.WriteLine($"@{username}{Host switch { null => "", _ => "@" + Host }}:{Love}=>{Love + Unit}");
                return;
            }
            var change = LoveChangeFlag();
            if (change||(change ==false && Unit < 0))
            {
                Console.WriteLine($"@{username}{Host switch { null =>"",_ =>"@"+Host}}:{Love}=>{Love + Unit}");
                Love += Unit;
                LoveChangedTime.Add(DateTime.Now);
                var json = JsonSerializer.Serialize(Program.Users);
                File.WriteAllText(@"config\memory.json", json);
            }
            else
            {
                Console.WriteLine($"@{username}@{Host}:{Love}=>{Love}");
            }
        }
        private bool LoveChangeFlag()
        {
            if(LoveChangedTime.Count > ChangeNumThreshold - 1)
            {
                var Flag = 0;
                foreach (var Time in LoveChangedTime)
                {
                    if((DateTime.Now - Time).TotalHours <ChangeTimeThreshold)
                    {
                        Flag++;
                    }
                }
                if (Flag >= 3)
                {
                    return false;
                }
                else 
                {
                    LoveChangedTime.RemoveAt(0);
                    return true;
                }
            }
            return true;
        }
        private const int ChangeTimeThreshold = 24;
        private const int ChangeNumThreshold = 3;
        public  List<DateTime> LoveChangedTime { get; private set; }
        public string username { get; private set; }
        public string Host { get; private set; }
        public int Love { get; private set; }
    }
    /// <summary>
    /// リアクション用
    /// </summary>
    internal class  ReAction
    {
       public enum ReactionType
        {
        Reply,
        ReAction,
        ReplyAndReaction,
        Registar,
        none
        }
        public ReactionType Type { get; set; }
        public string uId { get; set; }
        public string nId { get; set; }
        public string Emoji { get; set; }
        public string Text { get; set; }
        public string Visibility { get; set; }
        public string[] visibleUserIds { get; set; }
    }
    /// <summary>
    /// 判定とかに使うノート情報
    /// </summary>
    internal class NoteInfo
    {
        public string uId { get; set; }
        public string nId { get; set; }
        public string Text { get; set; }
        public bool IsNotMention { get; set; } = false;
        public string Visibility { get; set; }
        public string visibleUserIds { get; set; }
        public string username { get; set; }
        public string Host { get; set; }

    }
    public class Reactions_Create
    {
        public string i { get; set; }
        public string noteId { get; set; }
        public string reaction { get; set; }
    }
    public class Notes_Create
    {
        public string i { get; set; }
        public string text { get; set; }
        public string visibility { get; set; }
        public string[] visibleUserIds { get; set; }
        public string replyId { get; set; }
    }
    public class Notes_Create_2
    {
        public string i { get; set; }
        public string text { get; set; }
        public string visibility { get; set; }
    }
    public class Following_Create
    {
        public string i { get; set; }
        public string userId { get; set; }
    }
}
