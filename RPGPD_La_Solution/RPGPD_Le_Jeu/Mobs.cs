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
            int HP = 0;
            Random random = new Random();
            
            switch (difficulty)
            {
                case 1:
                    switch (choix)
                    {
                        case 1: // Gobelin
                            HP = random.Next(7, 11);
                            break;
                        case 2: // Squelette
                            HP = random.Next(6, 9);
                            break;
                        case 3: // Gros Rat
                            HP = random.Next(3, 7);
                            break;
                    }
                    break;
                case 2:
                    switch (choix)
                    {
                        case 1: // Orc
                            HP = random.Next(9, 15);
                            break;
                        case 2: // Slime
                            HP = random.Next(17, 21);
                            break;
                        case 3: // Mage Gobelin
                            HP = random.Next(8, 12);
                            break;
                    }
                    break;
                case 3:
                    switch (choix)
                    {
                        case 1: // Troll
                            HP = random.Next(19, 31 );
                            break;
                        case 2: // Champion squelette
                            HP = random.Next(15, 21);
                            break;
                        case 3: // Sirène
                            HP = 25;
                            break;
                    }
                    break;
            }
            return HP;
        } // Fin fonction GénérerHP

        public int Générer_Attaque(int difficulty, int choix)
        {
            int Attack = 0;
            Random random = new Random();

            switch (difficulty)
            {
                case 1:
                    switch (choix)
                    {
                        case 1: // Gobelin
                            Attack = random.Next(2, 4);
                            break;
                        case 2: // Squelette
                            Attack = random.Next(2, 6);
                            break;
                        case 3: // Gros Rat
                            Attack = random.Next(3, 7);
                            break;
                    }
                    break;
                case 2:
                    switch (choix)
                    {
                        case 1: // Orc
                            Attack = random.Next(3, 6);
                            break;
                        case 2: // Slime
                            Attack = random.Next(1, 8);
                            break;
                        case 3: // Mage Gobelin
                            Attack = random.Next(6, 9);
                            break;
                    }
                    break;
                case 3:
                    switch (choix)
                    {
                        case 1: // Troll
                            Attack = random.Next(5, 10);
                            break;
                        case 2: // Champion squelette
                            Attack = random.Next(7, 11);
                            break;
                        case 3: // Sirène
                            Attack = 5;
                            break;
                    }
                    break;
            }
            return Attack;
        } // Fin fonction GénérerAttaque

        public int Générer_EXP(int difficulty, int choix)
        {
            int exp = 0;
            Random random = new Random();

            switch (difficulty)
            {
                case 1:
                    switch (choix)
                    {
                        case 1: // Gobelin
                            exp = random.Next(1, 2);
                            break;
                        case 2: // Squelette
                            exp = random.Next(1, 2);
                            break;
                        case 3: // Gros Rat
                            exp = random.Next(1, 2);
                            break;
                    }
                    break;
                case 2:
                    switch (choix)
                    {
                        case 1: // Orc
                            exp = random.Next(1, 2);
                            break;
                        case 2: // Slime
                            exp = random.Next(1, 2);
                            break;
                        case 3: // Mage Gobelin
                            exp = random.Next(1, 2);
                            break;
                    }
                    break;
                case 3:
                    switch (choix)
                    {
                        case 1: // Troll
                            exp = random.Next(1, 2);
                            break;
                        case 2: // Champion squelette
                            exp = random.Next(1, 2);
                            break;
                        case 3: // Sirène
                            exp = 1;
                            break;
                    }
                    break;
            }
            return exp; 
        } // Fin fonction Générer EXP

        public int ChoixActionEnnemi(int difficulty, int choix, int PlayerHP, int PlayerClass)
        {



            return difficulty;
        } // Fin fonction ChoixActionEnnemi

    }
}
