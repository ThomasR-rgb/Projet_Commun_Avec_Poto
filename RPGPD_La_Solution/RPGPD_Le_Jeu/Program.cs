using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace RPGPD_Le_Jeu
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }

    public enum PlayerClass { Fighter, WhiteMage, DarkMage }
    public enum GameState { MainMenu, ClassSelection, EncounterSelect, Battle, Victory, GameOver }

    public partial class GameForm
    {
        private int _battlesWon = 0;

    
        // Alerte remaniement du combat par Thomas, et nouvellement remodellé par Félix
        
        // MÉGA IMPORTANT : La boucle de thomas est asynchrone, ce qui marche pas dans un programme visuel,
        // faque j'ai charcuté un peu le tout pour que ça marche, m'en veut pas Tom

        private async Task BattleParThomas(int difficulty, int playerClass, int choixDeLennemi, int nbPlayerPotion) // 1 potion qui full life
        {
            Mobs mob = new Mobs(difficulty, choixDeLennemi, playerClass); 
            Player player = new Player(playerClass);
            
            int EnnemiHP = mob.Générer_HP();
            int EnnemiMaxHP = EnnemiHP;
            int playerHP = _currentHP; 
            int playerMaxHP = player.MaxHPPlayerScale(difficulty, playerClass);
            int playerMP = _currentMP; 
            int playerMaxMP = player.BaseMPPlayer(difficulty, playerClass); 

            // Première update de l'UI sinon ça marche pas dans les visuels
            UpdateBattleUI(playerHP, playerMaxHP, playerMP, playerMaxMP, EnnemiHP, EnnemiMaxHP);
            Log("Battle Start!");

            bool AcourtDeMP = false;
            int playerPotion = nbPlayerPotion;
            int playerAction = 0;
            Random rand = new Random();
            int intDivinDuJoueur = 0;
            int intDivinDuMobs = 0;
            int EnnemiAction = 0;
            int playerSpellAction = 1; // à régler
            bool MagicSwordActive = false; // Exception des spells
            bool DivineGraceActive = false;
            bool ReflectActive = false;
            bool RoughSkinActive = false; // Exception de capa de monstre

            do
            {
                // IMPORTANT : J'attend ici le joueur clique pour refresh
                // J'ai donc remplacé la logique par une attente active à la place de passive


                // J'ai aussi mis ça pour éviter le spam des boutons
                _inputEnabled = true; 
                ToggleActionButtons(true); // Les montrer visuellement (en les grisant pas)
    
                _lastActionSelected = 0; 
    
                // J'attends l'imput
                while (_lastActionSelected == 0)
               {
                await Task.Delay(100); 
               }

               // J'AI REÇU L'IMPUT, LOCK-IN
               _inputEnabled = false;
                ToggleActionButtons(false); // Là je les grise

               playerAction = _lastActionSelected;
                _lastActionSelected = 0; 

                switch (playerAction)
                {
                    case 1: // Attaque
                        TriggerAttackAnim(true); // Synchro de l'UI
                        await Task.Delay(500);   // Pause pour l'animation

                        EnnemiAction = mob.ChoixActionEnnemi(playerHP, EnnemiHP);
                        intDivinDuJoueur = player.Generer_Attaque_Player(difficulty, playerClass);
                        intDivinDuMobs = mob.ResultatTourMobsDeBase(EnnemiAction, EnnemiHP);
                        if (EnnemiAction == 2)
                        {
                            intDivinDuJoueur = 0;
                            // Animation de bloque du joueur et de l'attaque du joueur
                            TriggerEffect("BLOCK"); // Synchro de l'UI
                            EnnemiHP = EnnemiHP - intDivinDuJoueur - intDivinDuMobs;
                        }
                        else if (EnnemiAction == 1)
                        {
                            intDivinDuJoueur = GérerMesFuckassExceptionsJ(MagicSwordActive, RoughSkinActive, intDivinDuJoueur);
                            intDivinDuMobs = GérerMesFuckassExceptionsM(ReflectActive, intDivinDuMobs);
                            EnnemiHP = EnnemiHP - intDivinDuJoueur;
                            Log($"You attack for {intDivinDuJoueur} damage."); // Synchro de l'UI
                            TriggerEffect("HIT"); // Synchro de l'UI

                            // Empêche le fait que les mobs peuvent tomber en dessous de 0 PV avant de mourir
                            if (EnnemiHP <= 0)
                            { EnnemiHP = 0; break; }
                            
                            // Attaque de l'ennemi après
                            TriggerAttackAnim(false); // Synchro de l'UI
                            await Task.Delay(500); // Synchro de l'UI
                            playerHP = playerHP - intDivinDuMobs;
                            Log($"Enemy hits you for {intDivinDuMobs} damage."); // Synchro de l'UI
                            
                            if (playerHP <= 0)
                            { Environment.Exit(0); }
                        }
                        else if (EnnemiAction == 3)
                        {
                            intDivinDuJoueur = GérerMesFuckassExceptionsJ(MagicSwordActive, RoughSkinActive, intDivinDuJoueur);
                            EnnemiHP = EnnemiHP - intDivinDuJoueur; // J'ai ajouté ça pour que ça sync mieux à cause du troisième case
                            Log($"You attack for {intDivinDuJoueur} damage."); // Synchro de l'UI

                            if (intDivinDuMobs == 106) // Stun
                            { intDivinDuJoueur = 0; intDivinDuMobs = 0; Log("You were STUNNED!"); }
                            else if (intDivinDuMobs == 107) // Rough skin
                            { /* Si l'ennemi est robuse, alors je l'affiche */ RoughSkinActive = true; Log("Enemy uses ROUGH SKIN."); }
                            else if (intDivinDuMobs == 108) // Erase
                            { 
                                // EnnemiHP = EnnemiHP - intDivinDuJoueur;
                                if (playerHP < player.MaxHPPlayerScale(difficulty, playerClass) / 2)
                                { playerHP = 0; Environment.Exit(0); }
                            }
                            else
                            {
                                if(intDivinDuMobs < 0)
                                {
                                    intDivinDuJoueur = 0;
                                    EnnemiHP = EnnemiHP - intDivinDuJoueur - intDivinDuMobs; // C'est quoi ça Thomas, moi être perdu
                                    // Empêche le overheal
                                    if(EnnemiHP > EnnemiMaxHP) EnnemiHP = EnnemiMaxHP;
                                }
                                else
                                {
                                    // J'ai enlevé "EnnemiHP = EnnemiHP - intDivinDuJoueur;" parce que j'ai changé le sync
                                    if (EnnemiHP <= 0)
                                    { EnnemiHP = 0; break; }
                                    else
                                    { 
                                        TriggerAttackAnim(false); await Task.Delay(500); // Synchro de l'UI
                                        playerHP = playerHP - intDivinDuMobs; 
                                    }
                                }
                            }
                        }
                            break;
                    case 2: // Block
                        EnnemiAction = mob.ChoixActionEnnemi(playerHP, EnnemiHP);
                        intDivinDuJoueur = player.Generer_Defense_Player(difficulty, playerClass);
                        mob.ResultatTourMobsDeBase(EnnemiAction, EnnemiHP);
                        Log("You Block!"); // Synchro de l'UI

                        if (EnnemiAction == 1)
                        {
                            TriggerAttackAnim(false); await Task.Delay(500); // Synchro de l'UI
                            intDivinDuMobs = mob.Générer_Attaque(EnnemiHP);
                            intDivinDuMobs = GérerMesFuckassExceptionsM(ReflectActive, intDivinDuMobs);
                            intDivinDuMobs = intDivinDuMobs / 2;
                            playerHP = playerHP + intDivinDuJoueur - intDivinDuMobs;
                            // Empêche le overheal
                            if(playerHP > playerMaxHP) playerHP = playerMaxHP;

                            if (playerHP <= 0)
                            { Environment.Exit(0); }
                        }
                        else if (EnnemiAction == 2)
                        {
                            playerHP = playerHP + intDivinDuJoueur;
                            // Empêche le overheal (Encore une autre fois)
                            if(playerHP > playerMaxHP) playerHP = playerMaxHP;

                            EnnemiHP = EnnemiHP - intDivinDuMobs;
                            if(EnnemiHP > EnnemiMaxHP) EnnemiHP = EnnemiMaxHP;
                        }
                        else if(EnnemiAction == 3)
                        {
                            intDivinDuMobs = mob.AbiliteSpecialeMob();
                            if (intDivinDuMobs == 106) // Stun
                            { intDivinDuJoueur = 0; intDivinDuMobs = 0; }
                            else if (intDivinDuMobs == 107) // Rough skin
                            { EnnemiHP = EnnemiHP - intDivinDuJoueur; RoughSkinActive = true; }
                            else if (intDivinDuMobs == 108) // Erase
                            {
                                if (playerHP < player.MaxHPPlayerScale(difficulty, playerClass) / 2)
                                { playerHP = 0; Environment.Exit(0); }
                            }
                            else
                            {
                                if (intDivinDuMobs < 0)
                                {
                                    intDivinDuJoueur = 0;
                                    EnnemiHP = EnnemiHP - intDivinDuMobs;
                                    if(EnnemiHP > EnnemiMaxHP) EnnemiHP = EnnemiMaxHP;
                                }
                                else
                                {
                                    intDivinDuMobs = intDivinDuMobs / 2;
                                    playerHP = playerHP - intDivinDuMobs;
                                }
                            }
                        }
                        break;
                    case 3: // Spell
                        TriggerAttackAnim(true); // Synchro de l'UI
                        await Task.Delay(500);
                        
                        // Utiliser le spell sélectionné dans le menu
                        playerSpellAction = _selectedSpellId;

                        // PPatcher les spells pour qu'ils consomment et non qu'ils fassent gagner du mana.
                        // On calcule le futur mana
                        int nextMP = player.ManaCostJoueur(playerClass, playerSpellAction, playerMP);
                        
                        // Si le mana tombait sous zero, EstCeQueCCook retourne true (c'est cook pour vrai)
                        AcourtDeMP = player.EstCeQueCCook(nextMP);

                        if (AcourtDeMP == true) { 
                            intDivinDuJoueur = 0; 
                            Log("Not enough Mana! Skip a turn, bitch!"); 
                        }
                        else {
                            // On applique le coût
                            playerMP = nextMP;
                            intDivinDuJoueur = player.spellJoueur(playerClass, playerSpellAction);
                        }

                        EnnemiAction = mob.ChoixActionEnnemi(playerHP, EnnemiHP);
                        intDivinDuMobs = mob.ResultatTourMobsDeBase(EnnemiAction, EnnemiHP);
                        if (intDivinDuMobs == 106) // Stun
                        { intDivinDuJoueur = 0; intDivinDuMobs = 0; }
                        if (EnnemiAction == 1) { intDivinDuMobs = GérerMesFuckassExceptionsM(ReflectActive, intDivinDuMobs); }
                        if (intDivinDuJoueur != 101 && intDivinDuJoueur != 102 && intDivinDuJoueur != 103 && intDivinDuJoueur != 104 && intDivinDuJoueur != 105)
                        {
                            if(intDivinDuJoueur < 0)
                            { 
                                playerHP = playerHP - intDivinDuJoueur; 
                                // Empêche le overheal (encore encore)
                                if(playerHP > playerMaxHP) playerHP = playerMaxHP;

                                TriggerEffect("HEAL", -intDivinDuJoueur); 
                            }
                            else
                            {
                                EnnemiHP = EnnemiHP - intDivinDuJoueur;
                                TriggerEffect("HIT");
                                if (EnnemiHP <= 0)
                                { EnnemiHP = 0; break; }
                            }
                        }
                        switch(intDivinDuJoueur)
                        {
                            case 101: // Magic sword
                                MagicSwordActive = true;
                                Log("Magic Sword Active!");
                                break;
                            case 102: // Divine grace
                                DivineGraceActive = true;
                                Log("Divine Grace Active!");
                                break;
                            case 103: // Vampire touch
                                intDivinDuJoueur = rand.Next(2, 6);
                                EnnemiHP = EnnemiHP - intDivinDuJoueur;
                                if(EnnemiHP < 0) EnnemiHP = 0;
                                playerHP = playerHP + intDivinDuJoueur;
                                if(playerHP > playerMaxHP) playerHP = playerMaxHP;
                                Log("Vampire Touch!");
                                break;
                            case 104: // reflect
                                ReflectActive = true;
                                Log("Reflect Active!");
                                break;
                            case 105: // kill
                                EnnemiHP = 0;
                                Log("KILL!");
                                break;
                        }

                        if (EnnemiAction == 2)
                        {
                            intDivinDuJoueur = 0;
                            // Animation de bloque du joueur et de l'attaque du joueur
                            EnnemiHP = EnnemiHP - intDivinDuJoueur - intDivinDuMobs;
                            if(EnnemiHP > EnnemiMaxHP) EnnemiHP = EnnemiMaxHP;

                            if (DivineGraceActive) { intDivinDuJoueur = player.MaxHPPlayerScale(difficulty, playerClass); }
                        }
                        else if (EnnemiAction == 1)
                        {
                            TriggerAttackAnim(false); await Task.Delay(500); // Synchro de l'UI
                            intDivinDuMobs = GérerMesFuckassExceptionsM(ReflectActive, intDivinDuMobs);
                            playerHP = playerHP - intDivinDuMobs;
                            Log($"Enemy hits you for {intDivinDuMobs}.");
                            if (DivineGraceActive) { intDivinDuJoueur = player.MaxHPPlayerScale(difficulty, playerClass); }
                            if (playerHP <= 0)
                            { Environment.Exit(0); }
                        }
                        else if (EnnemiAction == 3)
                        {
                            
                            if (intDivinDuMobs == 107) // Rough skin
                            { EnnemiHP = EnnemiHP - intDivinDuJoueur; RoughSkinActive = true; }
                            else if (intDivinDuMobs == 108) // Erase
                            {
                                if (playerHP < player.MaxHPPlayerScale(difficulty, playerClass) / 2)
                                { playerHP = 0; Environment.Exit(0); }
                            }
                            else
                            {
                                if (intDivinDuMobs < 0)
                                {
                                    intDivinDuJoueur = 0;
                                    EnnemiHP = EnnemiHP - intDivinDuJoueur - intDivinDuMobs;
                                    if(EnnemiHP > EnnemiMaxHP) EnnemiHP = EnnemiMaxHP;
                                }
                                else
                                {
                                    if (EnnemiHP <= 0)
                                    { EnnemiHP = 0; break; }
                                    else
                                    { playerHP = playerHP - intDivinDuMobs; }
                                }
                            }
                        }

                        break;
                    case 4: // Item
                        EnnemiAction = rand.Next(1, 3);
                        intDivinDuMobs = mob.ResultatTourMobsDeBase(EnnemiAction, EnnemiHP);
                        intDivinDuMobs = GérerMesFuckassExceptionsM(ReflectActive, intDivinDuMobs);
                        switch (EnnemiAction)
                        {
                            case 1:
                                TriggerAttackAnim(false); await Task.Delay(500); // Synchro de l'UI
                                playerHP = playerHP - intDivinDuMobs;
                                if (playerHP <= 0)
                                { Environment.Exit(0); }
                                if (playerPotion == 1) // 1 potion max cette fois-ci
                                {
                                    playerHP = player.MaxHPPlayerScale(difficulty, playerClass); // un full life
                                    TriggerEffect("HEAL", 100);
                                    Log("Potion used! Full Health.");
                                    playerPotion = 0;
                                    _potions = 0; // Sync de la variable visuelle
                                }
                                else {
                                     Log("No potions left!");
                                }
                                break;
                            case 2:
                                if (playerPotion == 1)
                                {
                                    playerHP = player.MaxHPPlayerScale(difficulty, playerClass); // un full life
                                    TriggerEffect("HEAL", 100);
                                    playerPotion = 0;
                                    _potions = 0; // Sync de la variable visuelle
                                }
                                EnnemiHP = EnnemiHP + intDivinDuMobs;
                                if(EnnemiHP > EnnemiMaxHP) EnnemiHP = EnnemiMaxHP;
                                break;
                        }
                        break;
                }
                
                // Update de l'UI, mais cette fois-ci pour ceux qui ont la BARRE

                UpdateBattleUI(playerHP, playerMaxHP, playerMP, playerMaxMP, Math.Max(0, EnnemiHP), EnnemiMaxHP);

            } while (EnnemiHP > 0);

            TriggerEffect("VICTORY");

            _currentHP = playerHP;
            _currentMP = playerMP;

        }  // Fin du tung tung tung sahur (Okay Thomas, my bad)

        public int GérerMesFuckassExceptionsJ(bool magicSwordActive, bool roughskinActive, int intdivindujoueur)
        {
            if (magicSwordActive == true) { intdivindujoueur = intdivindujoueur + 1; return intdivindujoueur; }
            if (roughskinActive == true) { intdivindujoueur = intdivindujoueur - 2; return intdivindujoueur; }
            return intdivindujoueur;
        }
        public int GérerMesFuckassExceptionsM(bool reflectActive, int intdivindumob) // C'est moi qui va te faire manger tes fuckass exceptions
        {
            if (reflectActive == true) { intdivindumob = intdivindumob - 2; }
            return intdivindumob;
        }
        
    }
}