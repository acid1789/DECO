using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    class AccountData
    {
        public List<CharData> Characters = new List<CharData>( );

        public AccountData(string Name)
        {
            if (!Directory.Exists("Accounts\\" + Name))
                return;

            string[] CharsFolders = Directory.GetFiles("Accounts\\" + Name);
            foreach (string Folder in CharsFolders)
            {
                string CharName = Path.GetFileNameWithoutExtension(Folder);
                if (CharName != "@Password")
                    Characters.Add(new CharData(null, Name, CharName));
            }
        }
    }
}
