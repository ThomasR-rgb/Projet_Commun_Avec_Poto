using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPD_Le_Jeu
{
    internal class Mobs
    {
        public int difficulty { get; set; }
        public int choixMob { get; set; }
        public int playerClass { get; set; }
        public Mobs (int difficulty, int choixMob, int playerClass)
        {
            this.difficulty = difficulty;
            this.choixMob = choixMob;
            this.playerClass = playerClass;
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
                        case 4: // Swarm gobelin (l'équivalent de 3 gobelins en même temps)
                            HP = random.Next(23, 31);
                            return HP;
                        case 5: // Porte Méchante (Angry Door)
                            HP = 20;
                            return HP;
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
                        case 4: // Géant (peut stun le joueur donc lui faire skipper son tour, si joueur a utilisé spell ou consommable, le joueur perd quand même les MP ou consommable)
                            HP = random.Next(30, 41);
                            return HP;
                        case 5: // Garguouille (peut réduire attaque physique reçue de 50%)
                            HP = random.Next(25, 31);
                            return HP;
                    }
                    break;
                case 3:
                    switch (choix)
                    {
                        case 1: // Troll
                            HP = random.Next(19, 31 );
                            return HP;
                        case 2: // Champion squelette
                            HP = random.Next(15, 21);
                            return HP;
                        case 3: // Sirène
                            HP = 25;
                            return HP;
                        case 4: // Démon Démoniaque (fait extrêmement mal, mais subit des dégats à chaque attaque, bloque très probable sur la verge de la mort)
                            HP = random.Next(35, 46);
                            return HP;
                        case 5: // Dark sorcerer (un sorcier qui a mal tourné (magic barrier?))
                            HP = random.Next(30, 36);
                            return HP;
                    }
                    break;
            }
            return HP;
        } // Fin fonction GénérerHP

        public int Générer_Attaque(int difficulty, int choix, int ennemiHP)
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
                        case 4: // Swarm de gobelins
                            if (ennemiHP >= 18)
                            { Attack = random.Next(6, 10); }
                            else if (ennemiHP > 19 && ennemiHP <= 9)
                            { Attack = random.Next(4, 7); }
                            else
                            { Attack = random.Next(2, 4); }
                            break;
                        case 5: // Porte méchante
                            Attack = random.Next(4, 7);
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
                        case 4: // Géant
                            Attack = random.Next(5, 9);
                            break;
                        case 5: // Gargouille
                            Attack = random.Next(7, 9);
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
                        case 4: // Démon démoniaque
                            Attack = random.Next(7, 11);
                            break;
                        case 5: // Dark Sorcerer
                            Attack = random.Next(6, 10);
                            break;
                    }
                    break;
            }
            return Attack;
        } // Fin fonction GénérerAttaque

        

        // Fonction Bloque des ennemis de base
        public int GénérerBloque(int difficulty, int choix, int ennemiHP)
        {
            int BloqueHeal = 0;
            Random random = new Random();

            switch(difficulty)
            {
                case 1:
                    switch(choix)
                    {
                        case 1:
                        case 2:
                        case 3:
                            BloqueHeal = random.Next(-3, -5);
                            break;
                        case 4: // Swarm de gobelins
                            if(ennemiHP >= 20)
                            { BloqueHeal = random.Next(-9, -13); }
                            else if (ennemiHP > 20 && ennemiHP <= 9)
                            { BloqueHeal = random.Next(-6, -9); }
                            else
                            { BloqueHeal = random.Next(-3, -5); }
                            break;
                        case 5: // Porte méchante
                            BloqueHeal = random.Next(-4, -6);
                            break;
                    }
                    break;
                case 2:
                    switch (choix)
                    {
                        case 1: // Orc
                            BloqueHeal = random.Next(-4, -7);
                            break;
                        case 2: // Slime
                            BloqueHeal = -6;
                            break;
                        case 3: // Mage Gobelin
                            BloqueHeal = random.Next(-6, -9);
                            break;
                        case 4: // Géant
                            BloqueHeal = random.Next(-3, -5);
                            break;
                        case 5: // Gargouille
                            BloqueHeal = random.Next(-5, -7);
                            break;
                    }
                    break;
                case 3:
                    switch (choix)
                    {
                        case 1: // Troll
                            BloqueHeal = random.Next(-7, -9);
                            break;
                        case 2: // Champion squelette
                            BloqueHeal = random.Next(-7, -9);
                            break;
                        case 3: // Sirène
                            BloqueHeal = -10;
                            break;
                        case 4: // Démon démoniaque
                            BloqueHeal = random.Next(-5, -7);
                            break;
                        case 5: // Dark Sorcerer
                            BloqueHeal = random.Next(-6, -8);
                            break;
                    }
                    break;
            }
            return BloqueHeal;
        }

        // Fonction pour l'IA des petits ennemis
        public int ChoixActionEnnemi(int PlayerHP, int MobHP) 
        {
            int ChoixEnnemi = 0;
            Random random = new Random();
            int aleatoire = 0;
            // Code d'action des ennemis: 1 = attaque,  2 = Bloque et régène, 3 = Abilité particulière au mob

            switch (difficulty)
            {
                case 1:
                    switch (choixMob)
                    {
                        case 1: // Gobelin
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 8 || PlayerHP < 4)
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
                            if (MobHP >= 7 || PlayerHP < 5)
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
                            if (MobHP >= 5 || PlayerHP < 6)
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
                    switch (choixMob)
                    {
                        case 1: // Orc
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 11 || PlayerHP < 4)
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
                            if (MobHP >= 15 || PlayerHP < 4)
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
                            if (MobHP >= 8 || PlayerHP < 4)
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
                    switch (choixMob)
                    {
                        case 1: // Troll
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 15 || PlayerHP < 6)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 3)
                                { ChoixEnnemi = 1; }
                                else if (aleatoire >= 6)
                                { ChoixEnnemi = 2; }
                                else
                                { ChoixEnnemi = 3; }
                            }
                            return ChoixEnnemi;
                        case 2: // Champion squelette
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 11 || PlayerHP < 10)
                            { ChoixEnnemi = 1; }
                            else
                            {
                                if (aleatoire <= 6)
                                { ChoixEnnemi = 1; }
                                else if (aleatoire == 7)
                                { ChoixEnnemi = 3; }
                                else
                                { ChoixEnnemi = 2; }
                            }
                            return ChoixEnnemi;
                        case 3: // Sirène
                            aleatoire = random.Next(0, 11);
                            if (MobHP >= 20 || PlayerHP < 6)
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
            }
            return ChoixEnnemi;
        } // Fin fonction ChoixActionEnnemi

        // Choisi le boss
        public int Generer_Choix_Boss()
        {
            Random random = new Random();
            int choix = random.Next(4, 5);
            return choix;
        }
        // Fin fonction choisi boss

        // Liste des abilités spéciales
        /* Angry door: door close -1 annd block damage
         * Mage gobelin: lightning 7-10 DP
         * Géant: Stun 106
         * Garguouille: Peau dure 107
         * Troll: Swing 50% = 0 DP  50% 10-15 DP
         * Sirène: Water song -8--13 heal
         * Dark Sorcerer: Erase si joueur est en bas de 50%, kill sinon rien
         */

        // Fonction abilité spéciale des mobs
        public int AbiliteSpecialeMob()
        {
            Random random = new Random();

            switch (difficulty)
            {
                case 1:
                    switch (choixMob)
                    {
                        case 1: // Gobelin
                            break;
                        case 2: // Squelette
                            break;
                        case 3: // Gros Rat
                            break;
                        case 4: // Goblin Swarm
                            break;
                        case 5: // Angry door
                            break;
                    }
                    break;
                case 2:
                    switch (choixMob)
                    {
                        case 1: // Orc
                            break;
                        case 2: // Slime
                            break;
                        case 3: // Mage Gobelin
                            break;
                        case 4: // Géant
                            break;
                        case 5: // Garguouille
                            break;
                    }
                    break;
                case 3:
                    switch (choixMob)
                    {
                        case 1: // Troll
                            break;
                        case 2: // Champion squelette
                            break;
                        case 3: // Sirène
                            break;
                        case 4: // Démon Démoniaque
                            break;
                        case 5: // Dark Sorcerer
                            break;
                    }
                    break;
            }
            return exp;
        

        }
        

        // Début fonction RésultatTourMobs
        public int ResultatTourMobsDeBase(int difficulty, int choix , int ChoixActionMobs, int ennemiHP)
        {
            int resultat = 0;
            switch (ChoixActionMobs)
            {
                case 1:
                    resultat = Générer_Attaque(difficulty, choix, ennemiHP);
                    return resultat;
                case 2:
                    resultat = GénérerBloque(difficulty, choix, ennemiHP);
                    return resultat;
                case 3:
                    resultat = 0;
                    return resultat;
            }
            return resultat;
        }
        // Fin fonction ResultatTourMobsDeBase

        /*
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         *  SERS PU A RIEN
         */

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


    }
}
