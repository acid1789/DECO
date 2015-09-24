using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecoPlayServer
{
    public enum CharJob : byte
    {
        Wanderer = 0,
        Stranger = 1,
        Swordman = 2,
        Archer = 3,
        Magician = 4,
        Shaman = 5,
        Knight = 6,
        Mercenary = 7,
        Slayer = 8,
        Sniper = 9,
        Prominas = 10,
        Priest = 11,
        Holy_Avenger = 12,
        Phyche = 13,
        Arm_Smith = 50,
        Armor_Smith = 51,
        Artisan = 52,
        Cook = 53,
        Merchant = 54,
        Leader = 60,
        Negotiator = 61,
        Bard = 62,
        Scientist = 63
    }

    public enum CharNation : byte
    {
        Millena = 0,
        Rain = 1,
    }

    public enum CharGender : byte
    {
        Male = 0,
        Female = 1,
    }
}
