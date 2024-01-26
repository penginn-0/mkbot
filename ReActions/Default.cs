using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace mkbot.ReActions
{
    internal class Default:Base
    {
        public Default()
        {
        }
        public static ReAction? GetReaction(NoteInfo note)
        {
            if (note.IsNotMention) { return null; }
            return new ReAction()
            {
                Type = ReAction.ReactionType.ReAction,
                nId = note.nId,
                uId = note.uId,
                Emoji = "❤️",
                Visibility = note.Visibility,
                visibleUserIds = new string[1] { note.uId }
            };

        }
    }
}
