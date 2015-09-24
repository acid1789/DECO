using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoLoginServer
{
    public enum LoginState : uint
    {
        Good = 0,
        AlreadyLogin = 1,
        NotID = 2,
        WrongPass = 3,
        NoGameServer = 4
    }

    class AccountControl
    {
        public static LoginState GetLoginState(string User, string Pass)
        {
            string AccountDirectory = null;

            User = User.ToUpper( );
            Pass = Pass.ToUpper( );
            List<string> Directories = Directory.GetDirectories("Accounts").ToList( );

            foreach (string x in Directories)
            {
                if (x.Equals("Accounts\\" + User, StringComparison.OrdinalIgnoreCase))
                {
                    AccountDirectory = x;
                    break;
                }
            }

            if (AccountDirectory == null)
                return LoginState.NotID;

            string RightPass = File.ReadAllText(AccountDirectory + "\\@Password");
            if (Pass != RightPass)
                return LoginState.WrongPass; 

            return LoginState.Good;
        }
    }
}
