using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mkbot.ReActions
{
    internal class Registar :Base
    {
        public Registar(List<string> keyword) : base(keyword)
        {
        }
        public ReAction? Check_RegistarMessage(NoteInfo note)
        {
            if (note.IsNotMention == true) { return null; }

            if (base.CheckKeyword(note.Text))
            {
                return new ReAction()
                {
                    Type = ReAction.ReactionType.Registar,
                    nId = note.nId,
                    uId = note.uId,
                };
            }
            return null;
        }
    }
}
    