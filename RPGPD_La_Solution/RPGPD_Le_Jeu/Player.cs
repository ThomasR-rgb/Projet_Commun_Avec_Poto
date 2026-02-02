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

    }
}
