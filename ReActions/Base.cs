using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mkbot;
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
        private int Unit ;
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
                user.CalcLove(Unit);
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
