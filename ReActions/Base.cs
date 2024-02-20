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
        public Base()
        {

        }
        public Base(List<string> keyword)
        {
            this.Keyword = keyword;
        }
        public Base(BaseArgs_Min Min)
        {
            Keyword = Min.Keyword;
            Emoji = Min.Emoji;
            Reply_Hate = Min.Hate;
            Reply_Normal = Min.Normal;
            Reply_Love = Min.Love;
            Unit = new (new int[] { User.LoveUnit0, User.LoveUnit1, User.LoveUnit2 });
        }
        public Base(BaseArgsOption0 Op0)
        {
            Keyword = Op0.Keyword;
            Emoji = Op0.Emoji;
            Reply_Hate = Op0.Hate;
            Reply_Normal = Op0.Normal;
            Reply_Love = Op0.Love;
            Unit = Op0.Unit;
        }
        public Base(BaseArgsOption1 Op1)
        {
            Keyword = Op1.Keyword;
            Emoji = Op1.Emoji;
            Reply_Hate = Op1.Hate;
            Reply_Normal = Op1.Normal;     
            Reply_Love = Op1.Love;
            NotMentionHate = Op1.NotMentionHate;
            NotMentionNormal = Op1.NotMentionNormal;
            NotMentionLove = Op1.NotMentionLove;
            NotMentionEmoji = Op1.NotMentionEmoji;
            Unit = new(new int[] { User.LoveUnit0, User.LoveUnit1, User.LoveUnit2 });
        }
        public Base(BaseArgsOption2 Op2)
        {
            Keyword = Op2.Keyword;
            Emoji = Op2.Emoji;
            Reply_Hate = Op2.Hate;
            Reply_Normal = Op2.Normal;
            Reply_Love = Op2.Love;
            NotMentionHate = Op2.NotMentionHate; 
            NotMentionNormal = Op2.NotMentionNormal;
            NotMentionLove = Op2.NotMentionLove;
            NotMentionEmoji = Op2.NotMentionEmoji;
            Unit = Op2.Unit;
        }
        private readonly List<int> Unit;
        public readonly List<string>  Keyword;
        public readonly List<string> Emoji;
        public readonly List<string> Reply_Hate;
        public readonly List<string> Reply_Normal;
        public readonly List<string> Reply_Love;
        public readonly List<string> NotMentionHate;
        public readonly List<string> NotMentionNormal;
        public readonly List<string> NotMentionLove;
        public readonly List<string> NotMentionEmoji;
        public bool CheckKeyword(string Text)
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
            if (CheckKeyword(note.Text))
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
                string Reply ="";
                string Emoji = "";
                var type = ReAction.ReactionType.none;
                if (note.IsNotMention)
                {
                    Emoji = NotMentionEmoji[(int)user.GetLoveLevel()];
                    Reply = user.GetLoveLevel() switch
                    {
                        User.LoveLevel.Hate => NotMentionHate[new Random().Next(NotMentionHate.Count)],
                        User.LoveLevel.Normal => NotMentionNormal[new Random().Next(NotMentionNormal.Count)],
                        User.LoveLevel.Love => NotMentionLove[new Random().Next(NotMentionLove.Count)],
                        _ => throw new Exception("なんかおかしい")//娘パイロットにおすすめされたので
                    };
                }
                else
                {
                    Reply = user.GetLoveLevel() switch
                    {
                        User.LoveLevel.Hate => Reply_Hate[new Random().Next(Reply_Hate.Count)],
                        User.LoveLevel.Normal => Reply_Normal[new Random().Next(Reply_Normal.Count)],
                        User.LoveLevel.Love => Reply_Love[new Random().Next(Reply_Love.Count)],
                        _ => throw new Exception("なんかおかしい")//娘パイロットにおすすめされたので
                    };
                    Emoji = this.Emoji[(int)user.GetLoveLevel()];
                }
                type = GetType(Reply, Emoji);
                if (type != ReAction.ReactionType.none) 
                {//リアクションか返信をするときのみ親密度を変動させる
                    user.CalcLove(Unit[(int)user.GetLoveLevel()]); 
                }
                return new ReAction()
                {
                    Type = type,
                    nId = note.nId,
                    Emoji = Emoji,
                    Text = $"@{note.username}"+$"{note.Host switch { null =>"",_ =>"@"+note.Host}}\r" + Reply,
                    Visibility = note.Visibility,
                    visibleUserIds = new string[1] { note.uId }
                };
            }
            return null;
        }

        public ReAction.ReactionType GetType(string Reply,string Emoji)
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
