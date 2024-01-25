using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mkbot;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mkbot.ReActions
{
    internal class Base
    {
        public Base(List<string> keyword, string emoji, List<string> Hate, List<string> Normal, List<string> Love, int Unit = User.LoveUnit)
        {
            Keyword = keyword;
            Emoji = emoji;
            Reply_Hate = Hate;
            Reply_Normal = Normal;
            Reply_Love = Love;
            NotMentionReply = false;
            NotMentionEmoji = "none";
            this.Unit = Unit;
        }
        public Base(List<string> keyword, string emoji, List<string> Hate, List<string> Normal, List<string> Love, bool NotMentionReply, string NotMentionEmoji, int Unit = User.LoveUnit)
        {
            Keyword = keyword;
            Emoji = emoji;
            Reply_Hate = Hate;
            Reply_Normal = Normal;
            Reply_Love = Love;
            this.NotMentionReply = NotMentionReply;
            this.NotMentionEmoji = NotMentionEmoji;
            this.Unit = Unit;
        }
        public Base(BaseArgs_Min Min)
        {
            Keyword = Min.Keyword;
            Emoji = Min.Emoji;
            Reply_Hate = Min.Hate;
            Reply_Normal = Min.Normal;
            Reply_Love = Min.Love;
            NotMentionReply = false;
            NotMentionEmoji = "none";
            Unit = User.LoveUnit;
        }
        public Base(BaseArgsOption0 Op0)
        {
            Keyword = Op0.Keyword;
            Emoji = Op0.Emoji;
            Reply_Hate = Op0.Hate;
            Reply_Normal = Op0.Normal;
            Reply_Love = Op0.Love;
            NotMentionReply = false;
            NotMentionEmoji = "none";
            Unit = Op0.Unit;
        }
        public Base(BaseArgsOption1 Op1)
        {
            Keyword = Op1.Keyword;
            Emoji = Op1.Emoji;
            Reply_Hate = Op1.Hate;
            Reply_Normal = Op1.Normal;
            Reply_Love = Op1.Love;
            NotMentionReply = Op1.NotMentionReply;
            NotMentionEmoji = Op1.NotMentionEmoji;
            Unit = User.LoveUnit;
        }
        public Base(BaseArgsOption2 Op2)
        {
            Keyword = Op2.Keyword;
            Emoji = Op2.Emoji;
            Reply_Hate = Op2.Hate;
            Reply_Normal = Op2.Normal;
            Reply_Love = Op2.Love;
            NotMentionReply = Op2.NotMentionReply;
            NotMentionEmoji = Op2.NotMentionEmoji;
            Unit = Op2.Unit;
        }
        private readonly int Unit;
        public readonly List<string>  Keyword;
        public readonly string Emoji;
        public readonly List<string> Reply_Hate;
        public readonly List<string> Reply_Normal;
        public readonly List<string> Reply_Love;
        public readonly bool NotMentionReply;
        public readonly string NotMentionEmoji;
        public bool CheckMessage(string Text)
        {
            foreach (var word in Keyword)
            {
                if (Text.Contains(word))
                {
                    return true;
                }
            }
            return false;
        }
        public ReAction? CheckMessage(NoteInfo note)
        {
            if (CheckMessage(note.Text))
            {
                var user = mkbot.Program.Users.Where(x => x.username == note.username && x.Host == note.Host).FirstOrDefault();
                if (user is null)
                {
                    return new ReAction()
                    {
                        Type = ReAction.ReactionType.ReAction,
                        Emoji = "❤️",
                        nId = note.nId
                    };
                }
                var Reply = user.GetLoveLevel() switch
                {
                    User.LoveLevel.Hate => Reply_Hate[new Random().Next(Reply_Hate.Count)],
                    User.LoveLevel.Normal => Reply_Normal[new Random().Next(Reply_Normal.Count)],
                    User.LoveLevel.Love => Reply_Love[new Random().Next(Reply_Love.Count)],
                    _ => throw new Exception("なんかおかしい")//要らないけどおすすめされたので
                };
                var type = GetType(Reply);
                var Emoji = this.Emoji;
                if (note.IsNotMention)
                {
                    if (NotMentionReply)
                    {
                    }
                    else
                    {
                        type = type switch
                        {
                            ReAction.ReactionType.Reply => ReAction.ReactionType.none,
                            ReAction.ReactionType.ReplyAndReaction => ReAction.ReactionType.ReAction,
                            _ => type
                        };
                        if (NotMentionEmoji == "none") { return null; }
                        Emoji = NotMentionEmoji;
                    }
                }
                if (type != ReAction.ReactionType.none) 
                {//リアクションか返信をするときのみ親密度を変動させる
                    user.CalcLove(Unit); 
                }
                return new ReAction()
                {
                    Type = type,
                    nId = note.nId,
                    Emoji = Emoji,
                    Text = $"@{note.username}@{note.Host}\r" + Reply,
                    Visibility = note.Visibility,
                    visibleUserIds = new string[1] { note.uId }
                };
            }
            return null;
        }

        public ReAction.ReactionType GetType(string Reply)
        {
            if (Emoji == "" && Reply == "")
            {
                return ReAction.ReactionType.none;
            }
            else if (Emoji != "" && Reply == "")
            {
                return ReAction.ReactionType.ReAction;
            }
            else if (Emoji == "" && Reply != "")
            {
                return ReAction.ReactionType.Reply;
            }
            else if (Emoji != "" && Reply != "")
            {
                return ReAction.ReactionType.ReplyAndReaction;
            }
            return ReAction.ReactionType.none;
        }
    }
}
