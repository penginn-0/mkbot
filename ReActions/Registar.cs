using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mkbot.ReActions
{
    internal class Registar :Base
    {
        public Registar(List<string> keyword, string emoji, List<string> Hate, List<string> Normal, List<string> Love) : base(keyword, emoji, Hate, Normal, Love)
        {
        }
        public ReAction? Check_RegistarMessage(NoteInfo note)
        {
            if (note.IsNotMention == true) { return null; }

            if (base.CheckMessage(note.Text))
            {
                var user = mkbot.Program.Users.Where(x => x.username == note.username && x.Host == note.Host).FirstOrDefault();
                if (user is null)
                {
                    return new ReAction()
                    {
                        Type = ReAction.ReactionType.Registar,
                        nId = note.nId,
                        uId = note.uId,
                        Emoji = "👍",
                        Visibility = note.Visibility,
                        visibleUserIds = new string[1] { note.uId }
                    };
                }
                else
                {
                    return new ReAction()
                    {
                        Type = ReAction.ReactionType.ReAction,
                        nId = note.nId,
                        Emoji = "❓",
                    };
                }
            }
            return null;
        }
    }
}
    