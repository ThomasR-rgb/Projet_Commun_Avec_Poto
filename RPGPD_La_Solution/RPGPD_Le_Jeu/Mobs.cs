using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPD_Le_Jeu
{
    internal class Mobs
    {
        public int HP;
        public int Attack;
        public int exp;
        public string Nom;
        public int difficulty;

        public Mobs (int HP, int Attack, int exp, int difficulty, string Nom)
        {
            this.HP = HP;
            this.Attack = Attack;
            this.exp = exp;
            this.difficulty = difficulty;
            this.Nom = Nom;
        }

        





    }
}
