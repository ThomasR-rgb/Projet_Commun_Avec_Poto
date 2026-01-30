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
        } // Fin fonction Générer XP

        // Fonction pour l'IA des petits ennemis
        public int ChoixActionEnnemi(int difficulty, int choix, int PlayerHP, int PlayerClass, int MobHP) 
        {
            int ChoixEnnemi = 0;
            Random random = new Random();
            int aleatoire = 0;
            // Code d'action des ennemis: 1 = attaque,  2 = Bloque et régène, 3 = Abilité particulière au mob

            switch (difficulty)
            {
                case 1:
                    switch (choix)
                    {
                        case 1: // Gobelin
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 8 || PlayerHP > 4)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 7)
                                { ChoixEnnemi = 1; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                                return ChoixEnnemi;
                        case 2: // Squelette
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 7 || PlayerHP > 5)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 8)
                                { ChoixEnnemi = 1; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                            return ChoixEnnemi;
                        case 3: // Gros Rat
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 5 || PlayerHP > 6)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 9)
                                { ChoixEnnemi = 1; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                            return ChoixEnnemi;
                    }
                    break;
                case 2:
                    switch (choix)
                    {
                        case 1: // Orc
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 11 || PlayerHP > 4)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 7)
                                { ChoixEnnemi = 1; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                            return ChoixEnnemi;
                        case 2: // Slime
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 15 || PlayerHP > 4)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 5)
                                { ChoixEnnemi = 1; }
                                else if (aleatoire == 10)
                                { ChoixEnnemi = 3; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                            return ChoixEnnemi;
                        case 3: // Mage Gobelin
                            aleatoire = random.Next(0, 11);
                            if (MobHP <= 8 || PlayerHP > 4)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 3)
                                { ChoixEnnemi = 1; }
                                else if (aleatoire == 5 || aleatoire == 6 || aleatoire == 4)
                                { ChoixEnnemi = 3; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                            return ChoixEnnemi;
                    }
                    break;
                case 3:
                    switch (choix)
                    {
                        case 1: // Troll
                            return ChoixEnnemi;
                        case 2: // Champion squelette
                            return ChoixEnnemi;
                        case 3: // Sirène
                            return ChoixEnnemi;
                    }
                    break;
            }
            return ChoixEnnemi;
        } // Fin fonction ChoixActionEnnemi

    }
}
