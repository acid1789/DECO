using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    public static class Extensions
    {
        public static PlayerOperation Find(this List<PlayerOperation> Operations, string Name)
        {
            for (int i = 0; i < Operations.Count; i++)
            {
                if (Operations[i].Name.Equals(Name, StringComparison.OrdinalIgnoreCase))
                    return Operations[i];
            }
            return null;
        }

        public static Player Find(this List<Player> Players, string AccountName, string CharName)
        {
            if (AccountName == null)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].CharName.Equals(CharName, StringComparison.OrdinalIgnoreCase))
                        return Players[i];
                }
            }
            else
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].AccountName.Equals(AccountName, StringComparison.OrdinalIgnoreCase))
                        return Players[i];
                }
            }
            return null;
        }

        public static Player Find(this List<Player> Players, Server Sock)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Sock.Sock.Equals(Sock.Sock))
                    return Players[i];
            }
            return null;
        }
    }

    public class PlayerOperation
    {
        public string Name = "";
        public List<object> Temp = new List<object>(4);
    }

    public class Player
    {
        public Server Sock = null;
        public uint ID = 0;
        public string AccountName = "";
        private string _CharName = "";
        public string CharName
        {
            get
            {
                return _CharName;
            }
            set
            {
                _CharName = value;
                CharData = new CharData(this, AccountName, _CharName);
            }
        }

        public CharData CharData = new CharData(null, "", "");

        public List<PlayerOperation> Operations = new List<PlayerOperation>( );
        public Player(Server Sock, uint ID)
        {
            this.Sock = Sock;
            this.ID = ID;
        }

        public void SendCharData( )
        {
            Packet LoginResponse = new Packet(0x0015);
            LoginResponse.WriteString(CharName, 17);
            LoginResponse.WriteString("hi everybody :))", 31);
            LoginResponse.WriteInt(Data.GetStyle(
                CharData.Gender,
                CharData.Nation,
                CharData.Face,
                CharData.Hair)); // style
            LoginResponse.WriteByte((byte)CharData.Job); // job
            LoginResponse.WriteByte(0);

            LoginResponse.WriteULong(CharData.EXP); // EXP
            LoginResponse.WriteByte(CharData.Level); // Level
            LoginResponse.WriteUInt(1);
            LoginResponse.WriteByte(1);
            LoginResponse.WriteUInt(CharData.Fame); // Fame
            LoginResponse.WriteUInt(CharData.NationRate); // NationRate

            LoginResponse.WriteUShort(CharData.Map); // Map
            LoginResponse.WriteInt(Data.GetGameCoord(CharData.Map, CharData.Coord)); // Coord
            LoginResponse.WriteUInt(CharData.CurHP); // CurHP 82
            LoginResponse.WriteUInt(CharData.CurSP); // CurSp 150
            LoginResponse.WriteUInt(CharData.CurMP); // CurMP
            LoginResponse.WriteByte(0);
            LoginResponse.WriteUShort(CharData.MovingSpeed); // Moving Speed  1 -> map size
            LoginResponse.WriteUShort(CharData.AbbillityMin);//PAMin
            LoginResponse.WriteUShort(CharData.AbbillityMax);//PAMax
            LoginResponse.WriteUShort(CharData.PhysicalDef); // PhysicalDef
            LoginResponse.WriteUShort(CharData.MagicalDef); // MagicalDef
            LoginResponse.WriteByte(8); // Attack Speed
            LoginResponse.WriteUShort(CharData.AbbillityMin); // MAMin
            LoginResponse.WriteUShort(CharData.AbbillityMax); // MAMax

            LoginResponse.WriteUInt(CharData.MaxHP); // MaxHP
            LoginResponse.WriteUInt(CharData.MaxSP); //MaxSP
            LoginResponse.WriteUInt(CharData.MaxMP); // MaxMP

            LoginResponse.WriteUShort(CharData.Power);//Power
            LoginResponse.WriteUShort(CharData.Vitality); // Vitality
            LoginResponse.WriteUShort(CharData.Sympathy); // Sympathy
            LoginResponse.WriteUShort(CharData.Intelligence); // Intelligence
            LoginResponse.WriteUShort(CharData.Stamina);    //Stamina
            LoginResponse.WriteUShort(CharData.Dexterity); // Dexterity
            LoginResponse.WriteByte(CharData.Charisma); // Charisma
            
            Random rd = new Random();
            int nLuck = rd.Next(-10, 10);
            LoginResponse.WriteByte((byte)nLuck); // Random Luck value

            LoginResponse.WriteUShort(CharData.AbilltyPoint); // Ability LvlUp Point

            LoginResponse.WriteUShort(CharData.LeftSP); // Left SP
            LoginResponse.WriteUShort(CharData.TotalSP); // Total SP

            LoginResponse.WriteUInt(CharData.WonPVPs); // WonPVPs
            LoginResponse.WriteUInt(CharData.TotalPVPs); // TotalPVPs

            LoginResponse.WriteUInt(CharData.Gold); // Gold
            LoginResponse.WriteUShort(0);

            LoginResponse.WriteInt(
                (CharData.ClothesItems.Count << 00) + // Clothes Items Count
                (CharData.GeneralItems.Count << 05) + // General Items Count
                (0 << 11) + // Items Count
                (0 << 17) + // Quest Items Count
                (CharData.Skills.Count << 22)); // Skills Count
            LoginResponse.WriteByte(1); // Buffs Count
            LoginResponse.WriteInt(
                (0 << 00) +
                (1 << 08) +
                (0 << 18) +
                (0 << 24)); // ?? Counts

            LoginResponse.WriteByte(3); // Boosters Count
            LoginResponse.WriteByte((byte)CharData.RidingItems.Count); // Riding Items Count
            LoginResponse.WriteByte(0); // Count

            LoginResponse.WriteByte(0x01);
            LoginResponse.WriteByte(0x01);
            LoginResponse.WriteByte(0x01);
            LoginResponse.WriteByte(0x01);
            LoginResponse.WriteByte(0x01);
            LoginResponse.WriteByte(0x01);
            LoginResponse.WriteByte(0x01);

            #region Clothes Items
            for (int i = 0; i < CharData.ClothesItems.Count; i++)
            {
                LoginResponse.WriteUInt(CharData.ClothesItems[i].ID);
                LoginResponse.WriteUShort(CharData.ClothesItems[i].Model); // Item Type
                LoginResponse.WriteBool(true);
                LoginResponse.WriteByte((byte)CharData.ClothesItems[i].Slot); // Item Slot
                LoginResponse.WriteUInt(1);
                LoginResponse.WriteUShort(CharData.ClothesItems[i].Durabillty); // Durability
                LoginResponse.WriteUShort(CharData.ClothesItems[i].RemainTime); // Remain Time
            }
            #endregion

            #region General Items
            for (int i = 0; i < CharData.GeneralItems.Count; i++)
            {
                LoginResponse.WriteUInt(CharData.GeneralItems[i].ID); // Item ID
                LoginResponse.WriteUShort(CharData.GeneralItems[i].Model); // Item Model
                LoginResponse.WriteBool(true);
                LoginResponse.WriteByte((byte)CharData.GeneralItems[i].Slot); // Item Slot
                LoginResponse.WriteUInt(1);
                LoginResponse.WriteUShort(CharData.GeneralItems[i].Durabillty); // Durabillty
                LoginResponse.WriteUShort(CharData.GeneralItems[i].RemainTime); // Remain Time
            }
            #endregion

            if (false)
            {
                #region General Items1
                for (int i = 0; i < 1; i++)
                {
                    LoginResponse.WriteUInt(11012);
                    LoginResponse.WriteUShort(11053); // Item ID
                    LoginResponse.WriteByte(0x04);
                    LoginResponse.WriteByte(0); // Item Slot
                    LoginResponse.WriteUInt(1);
                    LoginResponse.WriteUShort(1); // Durabillty
                    LoginResponse.WriteUShort(1); // Remain Time
                }
                #endregion

                #region Quest Items
                for (int i = 0; i < 1; i++)
                {
                    LoginResponse.WriteUInt(11012);
                    LoginResponse.WriteUShort(11053); // Item ID
                    LoginResponse.WriteByte(0x04);
                    LoginResponse.WriteByte(0); // Item Slot
                    LoginResponse.WriteUInt(1);
                    LoginResponse.WriteUShort(1); // Durabillty
                    LoginResponse.WriteUShort(1); // Remain Time
                }
                #endregion
            }

            #region Skills
            for (int i = 0; i < CharData.Skills.Count; i++)
            {
                LoginResponse.WriteUShort(CharData.Skills[i]); // Skill ID
            }
            #endregion

            LoginResponse.WriteUShort(125); // Buffs


            //LoginResponse.WriteUInt(525); //2<<0
            //LoginResponse.WriteByte(4);

            //LoginResponse.WriteUShort(0x720); //2<<18
            //LoginResponse.WriteByte(0x2B);

            LoginResponse.WriteUInt(1);
            LoginResponse.WriteUShort(1);
            LoginResponse.WriteString("dsadas", 70);

            LoginResponse.WriteUShort(0x720); //2<<8
            LoginResponse.WriteByte(3);


            /*LoginResponse.WriteUShort(8347); //2<<24
            LoginResponse.WriteByte(11);
            LoginResponse.WriteUShort(8347);
            LoginResponse.WriteByte(4);
            LoginResponse.WriteByte(4);*/

            #region Boosters
            LoginResponse.WriteUShort(11114); // Booster ID
            LoginResponse.WriteUInt(500);     // Booster Remain Time

            LoginResponse.WriteUShort(11115);
            LoginResponse.WriteUInt(500);

            LoginResponse.WriteUShort(11116);
            LoginResponse.WriteUInt(500);
            #endregion

            LoginResponse.WriteUInt(1);
            LoginResponse.WriteByte(5); // Frontier level
            LoginResponse.WriteString("hii", 15); // Frontier NickName
            LoginResponse.WriteUInt(1000); // Frontier MemberShipFee

            #region Riding Items
            for (int i = 0; i < CharData.RidingItems.Count; i++)
            {
                LoginResponse.WriteUInt(CharData.RidingItems[i].ID); // Item ID
                LoginResponse.WriteUShort(CharData.RidingItems[i].Model); // Item Model
                LoginResponse.WriteByte(1);
                LoginResponse.WriteByte((byte)CharData.RidingItems[i].Slot); // Item Slot
                LoginResponse.WriteUInt(2);
                LoginResponse.WriteUShort(CharData.RidingItems[i].Durabillty); // Durabillty
                LoginResponse.WriteUShort(CharData.RidingItems[i].RemainTime); // Remain Time
            }
            #endregion

            LoginResponse.WriteString("1234567890abcd", 14);
            
            Sock.Send(LoginResponse);
        }
    }
}
