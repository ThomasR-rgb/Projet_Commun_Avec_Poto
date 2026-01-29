using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPD_Le_Jeu
{
    internal class Mobs
    {
        public int difficulty;

        public Mobs (int difficulty)
        {
            this.difficulty = difficulty;
        }

        public int Générer_choix_Mob()
        {
            Random random = new Random();
            int choix = random.Next(1, 4);
            return choix;
        }
        public int Générer_HP(int difficulty, int choix)
        {
            
            switch (difficulty)
            {
                case 1:
                    switch (choix)
                    {
                        case 1:
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                    break;
                case 2:
                    break;
                case 3:
                    break;

            }
        }






    }
}
