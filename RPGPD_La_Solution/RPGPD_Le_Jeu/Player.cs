using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGPD_Le_Jeu
{
    internal class Player
    {
        public int playerClass;
        public Player (int playerClass)
        {
            this.playerClass = playerClass;
        }

        public int Generer_Attaque_Player(int playerClass, int playerLevel)
        {
            int attaquePlayer = 0;
            Random random = new Random();
            switch(playerClass)
            {
                case 1: // Fighter
                    switch(playerLevel)
                    {
                        case 1:
                            attaquePlayer = random.Next(2, 6);
                            return attaquePlayer;
                        case 2:
                            attaquePlayer = random.Next(3, 7);
                            return attaquePlayer;
                        case 3:
                            attaquePlayer = random.Next(4, 8);
                            return attaquePlayer;
                    }
                    break;
                case 2: // White Mage
                    switch (playerLevel)
                    {
                        case 1:
                            attaquePlayer = random.Next(1, 5);
                            return attaquePlayer;
                        case 2:
                            attaquePlayer = random.Next(2, 6);
                            return attaquePlayer;
                        case 3:
                            attaquePlayer = random.Next(3, 7);
                            return attaquePlayer;
                    }
                    break;
                case 3: // Dark Mage
                    switch (playerLevel)
                    {
                        case 1:
                            attaquePlayer = random.Next(2, 5);
                            return attaquePlayer;
                        case 2:
                            attaquePlayer = random.Next(3, 6);
                            return attaquePlayer;
                        case 3:
                            attaquePlayer = random.Next(4, 7);
                            return attaquePlayer;
                    }
                    break;
            }
            return attaquePlayer;
        } // Fin Generer_Attaque_Player

        public int Generer_Defense_Player(int playerClass, int playerLevel)
        {
            int bloquehealPlayer = 0;
            Random random = new Random();
            switch (playerClass)
            {
                case 1: // Fighter
                    switch (playerLevel)
                    {
                        case 1:
                            bloquehealPlayer = random.Next(1, 3);
                            return bloquehealPlayer;
                        case 2:
                            bloquehealPlayer = random.Next(2, 4);
                            return bloquehealPlayer;
                        case 3:
                            bloquehealPlayer = random.Next(3, 5);
                            return bloquehealPlayer;
                    }
                    break;
                case 2: // White Mage
                    switch (playerLevel)
                    {
                        case 1:
                            bloquehealPlayer = random.Next(1, 4);
                            return bloquehealPlayer;
                        case 2:
                            bloquehealPlayer = random.Next(3, 5);
                            return bloquehealPlayer;
                        case 3:
                            bloquehealPlayer = random.Next(4, 6);
                            return bloquehealPlayer;
                    }
                    break;
                case 3: // Dark Mage
                    switch (playerLevel)
                    {
                        case 1:
                            bloquehealPlayer = random.Next(1, 3);
                            return bloquehealPlayer;
                        case 2:
                            bloquehealPlayer = random.Next(2, 4);
                            return bloquehealPlayer;
                        case 3:
                            bloquehealPlayer = random.Next(3, 5);
                            return bloquehealPlayer;
                    }
                    break;
            } // Fin switch
            return bloquehealPlayer;
        } // Fin Generer_Defense_Player
        public int MaxHPPlayerScale(int difficulty, int playerClass)
        {
            int MaxHP = 0;
            switch(playerClass)
            {
                case 1: // Fighter
                    switch(difficulty) // level
                    {
                        case 1:
                            MaxHP = 25;
                            return MaxHP;
                        case 2:
                            MaxHP = 30;
                            return MaxHP;
                        case 3:
                            MaxHP = 35;
                            return MaxHP;
                    }
                    break;
                case 2: // White Mage
                    switch (difficulty) // level
                    {
                        case 1:
                            MaxHP = 22;
                            return MaxHP;
                        case 2:
                            MaxHP = 27;
                            return MaxHP;
                        case 3:
                            MaxHP = 31;
                            return MaxHP;
                    }
                    break;
                case 3: // Dark Mage
                    switch (difficulty) // level
                    {
                        case 1:
                            MaxHP = 20;
                            return MaxHP;
                        case 2:
                            MaxHP = 24;
                            return MaxHP;
                        case 3:
                            MaxHP = 28;
                            return MaxHP;
                    }
                    break;
            }
            return MaxHP;
        } // fin fonction Player HP Scale

        public int BaseMPPlayer(int difficulty, int playerclass)
        {
            int BaseMP = 0;
            switch(playerclass)
            {
                case 1: // Fighter
                    switch(difficulty)
                    {
                        case 1: // lvl1
                            BaseMP = 1;
                            return BaseMP;
                        case 2:
                            BaseMP = 1;
                            return BaseMP;
                        case 3:
                            BaseMP = 2;
                            return BaseMP;
                    }
                    break;
                case 2: // White Mage
                    switch (difficulty)
                    {
                        case 1: // lvl1
                            BaseMP = 2;
                            return BaseMP;
                        case 2:
                            BaseMP = 3;
                            return BaseMP;
                        case 3:
                            BaseMP = 5;
                            return BaseMP;
                    }
                    break;
                case 3: // Dark Mage
                    switch (difficulty)
                    {
                        case 1: // lvl1
                            BaseMP = 3;
                            return BaseMP;
                        case 2:
                            BaseMP = 5;
                            return BaseMP;
                        case 3:
                            BaseMP = 7;
                            return BaseMP;
                    }
                    break;
            }
            return BaseMP;
        }

        /* liste des spells possible
         * fighter: lvl1 magic sword +1DP sur attack jusqua fin combat
         *          lvl2 cure +10-15 HP
         * White mage: lvl1 1MP Fire Bolt 5-10DP   1MP Heal +8-13 HP
         *             lvl2 2MP Divine Grace si meurt le tour où le spell est cast, reviens a la vie avec 50% HP
         *             lvl3 3MP Smite 10-50DP
         * Dark mage: lvl1 1MP Vampire Touch 2-5DP + Heal 
         *            lvl2 2MP Reflect -[2-3] DP physique reçue jusqu'à la fin du combat  2MP Ice Beam 9-14DP
         *            lvl3 7MP Kill Tue le truc en face
         */
        public int spellJoueur(int playerclass)
        {
            int intdivinduspell = 0;
            switch(playerclass)
            {
                
            }
            return intdivinduspell;
        }

    } // pas sortir de ça
}
