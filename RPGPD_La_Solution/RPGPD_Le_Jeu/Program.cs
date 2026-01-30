using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace RPGPD_Le_Jeu
{
    // Classe principale qui lance l'application
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }

    // LOGIQUE ULTIME DU JEU ¯\_(ツ)_/¯

    // Les classes différentes
    public enum PlayerClass
    {
        Fighter,
        WhiteMage,
        DarkMage
    }

    // L'état du jeu
    public enum GameState
    {
        MainMenu,
        ClassSelection,
        EncounterSelect, // Ma nouvelle phase de sélection
        Battle,
        GameOver
    }

    // INTERFACE GRAPHIQUE !
    public class GameForm : Form
    {
        // VARIABLES DU JEU (Ce que Tom a demandé)
        private PlayerClass _currentClass;
        private int _playerHP;
        private int _playerMaxHP;
        private int _playerMana;
        private int _playerMaxMana;
        private int _playerLevel;
        private int _battlesWon;
        private int _potions;

        // Ennemi actuel (yen a juste un faque ouais)
        private string _enemyName = "Goblin";
        private int _enemyHP;
        private int _enemyMaxHP;
        private int _enemyDamage;
        
        // Variables pour gérer la logique Mobs.cs
        private int _leftMobId;
        private int _rightMobId;
        private int _selectedMobId;

        // Variable pour savoir si l'ennemi bloque venant de Mobs.cs
        private bool _enemyIsBlocking;

        // État actuel
        private GameState _currentState;

        // TOUS LES UI
        // On ajoute "= null!;" pour dire au compilateur qu'on va les initialiser plus tard
        private Label lblTitle = null!;
        private Button btnStart = null!;
        private Button btnQuit = null!;
        
        // UI de Sélection de classe
        private GroupBox grpClassSelect = null!;
        private Button btnFighter = null!;
        private Button btnWhiteMage = null!;
        private Button btnDarkMage = null!;

        // UI de Sélection de Rencontre
        private Panel pnlEncounterSelect = null!;
        private Button btnPathLeft = null!;
        private Button btnPathRight = null!;
        private string _leftEnemyType = "";
        private string _rightEnemyType = "";

        // UI de Combat
        private Panel pnlBattleScene = null!;
        private Label lblPlayerSprite = null!; 
        private Label lblEnemySprite = null!;  
        private Label lblPlayerStats = null!;
        private Label lblEnemyStats = null!;
        
        private GroupBox grpActions = null!;   
        private Button btnAttack = null!;
        private Button btnBlock = null!;
        private Button btnSpell = null!;
        private Button btnItem = null!;
        
        private RichTextBox txtGameLog = null!;

        public GameForm()
        {
            // Configuration de la fenêtre
            this.Text = "RudacoPG - The Adventure";
            this.Size = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Le fond il est TOUT NOIR
            this.BackColor = Color.Black; 

            InitializeComponents();
            ShowMainMenu();
        }

        // Initialisation de tous les widgets (boutons, textes, etc.)
        private void InitializeComponents()
        {
            // 1. Titre et Menu Principal
            lblTitle = new Label();
            lblTitle.Text = "RudacoPG";
            lblTitle.Font = new Font("Courier New", 36, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(280, 50);
            lblTitle.ForeColor = Color.White;
            this.Controls.Add(lblTitle);

            btnStart = CreateButton("Play", 300, 200, (s, e) => ShowClassSelection());
            btnQuit = CreateButton("Quit", 300, 270, (s, e) => Application.Exit());

            // 2. Sélection de Classe
            grpClassSelect = new GroupBox();
            grpClassSelect.Text = "Choose your Class";
            grpClassSelect.ForeColor = Color.White;
            grpClassSelect.Size = new Size(400, 300);
            grpClassSelect.Location = new Point(200, 150);
            grpClassSelect.Visible = false;
            
            btnFighter = CreateClassButton("Fighter (High HP/Atk)", 50);
            btnFighter.Click += (s, e) => StartGame(PlayerClass.Fighter);
            
            btnWhiteMage = CreateClassButton("White Mage (Heal/Def)", 110);
            btnWhiteMage.Click += (s, e) => StartGame(PlayerClass.WhiteMage);

            btnDarkMage = CreateClassButton("Dark Mage (High Dmg Spell)", 170);
            btnDarkMage.Click += (s, e) => StartGame(PlayerClass.DarkMage);

            grpClassSelect.Controls.Add(btnFighter);
            grpClassSelect.Controls.Add(btnWhiteMage);
            grpClassSelect.Controls.Add(btnDarkMage);
            this.Controls.Add(grpClassSelect);

            // Split le screen
            pnlEncounterSelect = new Panel();
            pnlEncounterSelect.Dock = DockStyle.Fill; // Prend toute la place
            pnlEncounterSelect.BackColor = Color.Black;
            pnlEncounterSelect.Visible = false;
            this.Controls.Add(pnlEncounterSelect);

            // Bouton Gauche (Screen splité)
            btnPathLeft = new Button();
            btnPathLeft.Location = new Point(0, 0);
            btnPathLeft.Size = new Size(400, 600); // Moitié gauche
            btnPathLeft.FlatStyle = FlatStyle.Flat;
            btnPathLeft.FlatAppearance.BorderColor = Color.Gray;
            btnPathLeft.BackColor = Color.FromArgb(20, 20, 20);
            btnPathLeft.ForeColor = Color.White;
            btnPathLeft.Font = new Font("Courier New", 16, FontStyle.Bold);
            btnPathLeft.Click += (s, e) => SelectPath(true);
            pnlEncounterSelect.Controls.Add(btnPathLeft);

            // Bouton Droit (Screen splité)
            btnPathRight = new Button();
            btnPathRight.Location = new Point(400, 0);
            btnPathRight.Size = new Size(400, 600); // Moitié droite
            btnPathRight.FlatStyle = FlatStyle.Flat;
            btnPathRight.FlatAppearance.BorderColor = Color.Gray;
            btnPathRight.BackColor = Color.FromArgb(20, 20, 20);
            btnPathRight.ForeColor = Color.White;
            btnPathRight.Font = new Font("Courier New", 16, FontStyle.Bold);
            btnPathRight.Click += (s, e) => SelectPath(false);
            pnlEncounterSelect.Controls.Add(btnPathRight);

            // 3. Phase de pétage de yeules
            pnlBattleScene = new Panel();
            pnlBattleScene.Size = new Size(760, 300);
            pnlBattleScene.Location = new Point(12, 12);
            // Le fond il est TOUT NOIR
            pnlBattleScene.BackColor = Color.Black;
            pnlBattleScene.BorderStyle = BorderStyle.Fixed3D;
            pnlBattleScene.Visible = false;
            this.Controls.Add(pnlBattleScene);

            // Les gros cubes
            lblEnemySprite = new Label { BackColor = Color.IndianRed, Size = new Size(100, 100), Location = new Point(600, 50), Text = "ENEMY", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblEnemySprite);

            lblPlayerSprite = new Label { BackColor = Color.SteelBlue, Size = new Size(100, 100), Location = new Point(100, 150), Text = "YOU", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblPlayerSprite);

            // Stats Affichées
            lblEnemyStats = new Label { Text = "Goblin Lvl 1\nHP: 20/20", AutoSize = true, Location = new Point(400, 50), Font = new Font("Consolas", 12), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblEnemyStats);

            lblPlayerStats = new Label { Text = "Hero Lvl 1\nHP: 50/50\nMP: 10/10", AutoSize = true, Location = new Point(220, 200), Font = new Font("Consolas", 12), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblPlayerStats);

            // 4. Zone de Log (texte en bas)
            txtGameLog = new RichTextBox();
            txtGameLog.Location = new Point(12, 320);
            txtGameLog.Size = new Size(400, 230);
            txtGameLog.ReadOnly = true;
            txtGameLog.BackColor = Color.Black;
            txtGameLog.ForeColor = Color.White;
            txtGameLog.Font = new Font("Consolas", 10);
            txtGameLog.Visible = false;
            this.Controls.Add(txtGameLog);

            // 5. Menu d'action (les 4 boutons)
            grpActions = new GroupBox();
            grpActions.Text = "Command";
            grpActions.ForeColor = Color.White;
            grpActions.Location = new Point(430, 320);
            grpActions.Size = new Size(340, 230);
            grpActions.Visible = false;
            this.Controls.Add(grpActions);

            btnAttack = CreateActionButton("ATTACK", 20, 30);
            btnAttack.Click += (s, e) => PlayerTurn("Attack");

            btnBlock = CreateActionButton("BLOCK", 180, 30);
            btnBlock.Click += (s, e) => PlayerTurn("Block");

            btnSpell = CreateActionButton("SPELL", 20, 130);
            btnSpell.Click += (s, e) => PlayerTurn("Spell");

            btnItem = CreateActionButton("ITEM", 180, 130);
            btnItem.Click += (s, e) => PlayerTurn("Item");

            grpActions.Controls.AddRange(new Control[] { btnAttack, btnBlock, btnSpell, btnItem });
        }

        // Helper pour créer des boutons de menu
        private Button CreateButton(string text, int x, int y, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(200, 50);
            btn.Font = new Font("Arial", 14);
            btn.BackColor = Color.Black;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.White;
            
            btn.Click += onClick;
            this.Controls.Add(btn);
            return btn;
        }

        // Helper des boutons partie 2
        private Button CreateClassButton(string text, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(50, y);
            btn.Size = new Size(300, 50);
            btn.BackColor = Color.Black;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.White;
            return btn;
        }

        // Helper des boutons partie 3
        private Button CreateActionButton(string text, int x, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(140, 80);
            btn.Font = new Font("Arial", 12, FontStyle.Bold);
            btn.BackColor = Color.Black;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.White;
            return btn;
        }

        // GESTION DES ÉCRANS
        private void ShowMainMenu()
        {
            _currentState = GameState.MainMenu;
            lblTitle.Visible = true;
            btnStart.Visible = true;
            btnQuit.Visible = true;
            grpClassSelect.Visible = false;
            pnlEncounterSelect.Visible = false;
            HideBattleUI();
        }

        private void ShowClassSelection()
        {
            _currentState = GameState.ClassSelection;
            btnStart.Visible = false;
            btnQuit.Visible = false;
            grpClassSelect.Visible = true;
        }
        
        // Affiche l'écran
        private void ShowEncounterSelection()
        {
            _currentState = GameState.EncounterSelect;
            grpClassSelect.Visible = false;
            HideBattleUI();
            pnlEncounterSelect.Visible = true;
            
            Random rnd = new Random();

            // On génère les ennemis en utilisant Mobs.cs
            // On limite la difficulté à 3 car Mobs.cs en a juste 3 et que Thomas est paresseux
            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;

            Mobs mobGenerator = new Mobs(currentDiff);

            _leftMobId = mobGenerator.Générer_choix_Mob();
            _rightMobId = mobGenerator.Générer_choix_Mob();

            _leftEnemyType = GetMobName(currentDiff, _leftMobId);
            _rightEnemyType = GetMobName(currentDiff, _rightMobId);

            // 15% de boumboclaat
            bool leftJammed = rnd.Next(0, 100) < 15;
            bool rightJammed = rnd.Next(0, 100) < 15;

            // Checker si c'est jammed
            string leftText = leftJammed ? "?? JAMMED ??" : _leftEnemyType;
            string rightText = rightJammed ? "?? JAMMED ??" : _rightEnemyType;

            // On update
            btnPathLeft.Text = $"PATH A\n\nStrange Corridor.\n\nSenses detect:\n{leftText}";
            btnPathRight.Text = $"PATH B\n\nDark Corridor...\n\nSenses detect:\n{rightText}";
            
            // Update les couleurs
            if (leftJammed) 
                btnPathLeft.ForeColor = Color.Gray;
            else 
                btnPathLeft.ForeColor = Color.White;

            if (rightJammed) 
                btnPathRight.ForeColor = Color.Gray;
            else 
                btnPathRight.ForeColor = Color.White;
        }

        // Fonction pour récupérer le nom depuis l'ID d'après Mobs.cs
        private string GetMobName(int difficulty, int choix)
        {
            switch (difficulty)
            {
                case 1:
                    if (choix == 1) return "Gobelin";
                    if (choix == 2) return "Squelette";
                    if (choix == 3) return "Gros Rat";
                    break;
                case 2:
                    if (choix == 1) return "Orc";
                    if (choix == 2) return "Slime";
                    if (choix == 3) return "Mage Gobelin";
                    break;
                case 3:
                    if (choix == 1) return "Troll";
                    if (choix == 2) return "Champion squelette";
                    if (choix == 3) return "Sirène";
                    break;
            }
            return "Unknown Horror";
        }

        private void HideBattleUI()
        {
            pnlBattleScene.Visible = false;
            txtGameLog.Visible = false;
            grpActions.Visible = false;
        }

        private void ShowBattleUI()
        {
            lblTitle.Visible = false;
            grpClassSelect.Visible = false;
            pnlEncounterSelect.Visible = false; // Cache la sélection quand le combat commence
            pnlBattleScene.Visible = true;
            txtGameLog.Visible = true;
            grpActions.Visible = true;
        }


        // LOGIQUE DU JEU (encore)

        private void StartGame(PlayerClass selectedClass)
        {
            _currentClass = selectedClass;
            _playerLevel = 1;
            _battlesWon = 0;
            _potions = 3;

            // Stats de base selon la classe
            switch (_currentClass)
            {
                case PlayerClass.Fighter:
                    _playerMaxHP = 25; _playerMaxMana = 20; break;
                case PlayerClass.WhiteMage:
                    _playerMaxHP = 22; _playerMaxMana = 80; break;
                case PlayerClass.DarkMage:
                    _playerMaxHP = 18; _playerMaxMana = 100; break;
                default: _playerMaxHP = 20; _playerMaxMana = 50; break;
            }
            _playerHP = _playerMaxHP;
            _playerMana = _playerMaxMana;

            Log("You chose " + _currentClass.ToString() + "!");
            
            // Ma belle map 
            ShowEncounterSelection();
        }

        // Callback quand tu choisis un chemin
        private void SelectPath(bool isLeft)
        {
            string enemyName = isLeft ? _leftEnemyType : _rightEnemyType;
            _selectedMobId = isLeft ? _leftMobId : _rightMobId;
            StartBattle(enemyName);
        }

        private void StartBattle(string enemyType)
        {
            _currentState = GameState.Battle;
            ShowBattleUI();

            _enemyName = enemyType;
            _enemyIsBlocking = false; // Reset de l'état de block
            
            // Scaling de base
            // Utilisation de Mobs.cs pour les stats
            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;
            
            Mobs mobGenerator = new Mobs(currentDiff);
            
            // Génération des HP et Attaque via la classe Mobs
            _enemyMaxHP = mobGenerator.Générer_HP(currentDiff, _selectedMobId);
            _enemyDamage = mobGenerator.Générer_Attaque(currentDiff, _selectedMobId);
            _enemyHP = _enemyMaxHP;

            // Boss Scaling
            // (La logique visuelle pour différencier les mobs plus forts)
            if (currentDiff == 3) // Si on est en difficulty 3, c'est du louuurd
            {
                lblEnemySprite.BackColor = Color.DarkRed;
                lblEnemySprite.Size = new Size(150, 150);
            }
            else // Goblin et autres
            {
                lblEnemySprite.BackColor = Color.IndianRed;
                lblEnemySprite.Size = new Size(100, 100);
            }

            UpdateStatsUI();
            Log($"--- BATTLE {_battlesWon + 1} START ---");
            Log($"You encounter a {_enemyName}!");
        }

        private void Log(string message)
        {
            txtGameLog.AppendText($"> {message}\n");
            txtGameLog.ScrollToCaret();
        }

        private void UpdateStatsUI()
        {
            lblPlayerStats.Text = $"YOU (Lvl {_playerLevel})\nHP: {_playerHP}/{_playerMaxHP}\nMP: {_playerMana}/{_playerMaxMana}\nPotions: {_potions}";
            lblEnemyStats.Text = $"{_enemyName}\nHP: {_enemyHP}/{_enemyMaxHP}";
        }

        // Tour du Joueur
        private void PlayerTurn(string action)
        {
            if (_currentState != GameState.Battle) return;

            bool playerActed = true;
            int damageDealt = 0;
            bool isBlocking = false;

            switch (action)
            {
                case "Attack":
                    // Dégâts basés sur la classe
                    // On met les dégâts aléatoires
                    Random rnd = new Random();
                    if (_currentClass == PlayerClass.Fighter)
                    {
                        damageDealt = rnd.Next(3, 7); // Entre 3 et 6 inclus
                    }
                    else
                    {
                        damageDealt = rnd.Next(2, 5); // Entre 2 et 4 inclus
                    }

                    // Coups Critiques 4% de chance
                    bool isCrit = rnd.Next(0, 100) < 4;
                    if (isCrit)
                    {
                        damageDealt *= 2;
                        Log("CRITICAL HIT! You hit a weak spot!");
                    }
                    
                    // Checker si l'ennemi bloque (venant de Mobs.cs encore une fois)
                    if (_enemyIsBlocking)
                    {
                        damageDealt /= 2;
                        Log("Enemy is guarding! Damage halved.");
                    }

                    _enemyHP -= damageDealt;
                    Log($"You attacked for {damageDealt} damage!");
                    break;

                case "Block":
                    isBlocking = true;
                    Log("You brace yourself for impact (Defense UP)!");
                    // Récupère un ti peu de mana
                    _playerMana = Math.Min(_playerMana + 5, _playerMaxMana);
                    break;

                case "Spell":
                    if (_playerMana >= 10)
                    {
                        _playerMana -= 10;
                        if (_currentClass == PlayerClass.WhiteMage)
                        {
                            int heal = 25 + (_playerLevel * 5);
                            _playerHP = Math.Min(_playerHP + heal, _playerMaxHP);
                            Log($"You cast HEAL! Recovered {heal} HP.");
                        }
                        else if (_currentClass == PlayerClass.DarkMage)
                        {
                            int spellDmg = 25 + (_playerLevel * 5);
                            
                            // Les spells peuvent pas être bloqués ? Ok Thomas
                            if (_enemyIsBlocking) {
                                spellDmg /= 2;
                                Log("Enemy magic resistance UP! Damage halved.");
                            }

                            _enemyHP -= spellDmg;
                            Log($"You cast FIREBALL! Dealt {spellDmg} damage.");
                        }
                        else // Fighter spell (faible)
                        {
                            int spellDmg = 15;
                            if (_enemyIsBlocking) spellDmg /= 2;
                            _enemyHP -= spellDmg;
                            Log($"You cast PUNCH SPELL! Dealt {spellDmg} damage.");
                        }
                    }
                    else
                    {
                        Log("Not enough Mana!");
                        playerActed = false; // Annule le tour
                    }
                    break;

                case "Item":
                    if (_potions > 0)
                    {
                        _potions--;
                        _playerHP = Math.Min(_playerHP + 50, _playerMaxHP);
                        Log("Used a Potion! +50 HP.");
                    }
                    else
                    {
                        Log("No potions left!");
                        playerActed = false;
                    }
                    break;
            }

            if (playerActed)
            {
                UpdateStatsUI();
                CheckBattleStatus(isBlocking);
            }
        }

        private void CheckBattleStatus(bool isBlocking)
        {
            if (_enemyHP <= 0)
            {
                // Victoire, HELL YEAAAH
                _enemyHP = 0;
                UpdateStatsUI();
                Log($"You defeated the {_enemyName}!");
                WinBattle();
            }
            else
            {
                // Tour de l'ennemi (simple délai simulé)
                EnemyTurn(isBlocking);
            }
        }

        private async void EnemyTurn(bool playerBlocking)
        {
            // Petit délai pour le réalisme
            grpActions.Enabled = false; // Empêche de cliquer
            await Task.Delay(1000);

            // Utilisation de Mobs.cs pour l'IA
            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;

            Mobs mobAI = new Mobs(currentDiff);
            
            // On demande à Mobs.cs quoi faire (1=Atk, 2=Block, 3=Special)
            int actionCode = mobAI.ChoixActionEnnemi(currentDiff, _selectedMobId, _playerHP, (int)_currentClass, _enemyHP);

            // On reset le block précédent car l'ennemi choisit une nouvelle action
            _enemyIsBlocking = false;

            if (actionCode == 1) // Attaque
            {
                int dmg = _enemyDamage;

                // Chance de critique ennemie 4%
                Random rnd = new Random();
                bool isCrit = rnd.Next(0, 100) < 4;

                if (isCrit)
                {
                    dmg *= 2;
                    Log("CRITICAL HIT! Enemy smashed you!");
                }

                if (playerBlocking) dmg /= 2; // Dégâts réduits de moitié si Bloque

                _playerHP -= dmg;
                Log($"The {_enemyName} attacks! You take {dmg} damage.");
            }
            else if (actionCode == 2) // Block et Regen
            {
                _enemyIsBlocking = true; // Sera utilisé au prochain tour du joueur
                
                int regen = 5 + (currentDiff * 2);
                _enemyHP = Math.Min(_enemyHP + regen, _enemyMaxHP);
                
                Log($"The {_enemyName} takes a defensive stance and regenerates {regen} HP!");
            }
            else if (actionCode == 3) // Abilité spéciale
            {
                int dmg = (int)(_enemyDamage * 1.5); // 50% plus mal
                if (playerBlocking) dmg /= 2;

                _playerHP -= dmg;
                Log($"!!! The {_enemyName} uses a SPECIAL ABILITY! You take CRITICAL {dmg} damage!");
            }
            
            UpdateStatsUI();

            if (_playerHP <= 0)
            {
                _playerHP = 0;
                UpdateStatsUI();
                GameOver();
            }
            
            grpActions.Enabled = true;
        }

        private void WinBattle()
        {
            _battlesWon++;
            
            // Level Up Check (Tous les 3 combats)
            int levelUpThreshold = 3; 
            
            MessageBox.Show($"Victory! You won battle #{_battlesWon}.", "Victory", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (_battlesWon % levelUpThreshold == 0)
            {
                _playerLevel++;
                _playerMaxMana += 5;
                _playerHP = _playerMaxHP; // Soin complet au level up
                _playerMana = _playerMaxMana;
                Log($"LEVEL UP! You are now level {_playerLevel}. Stats increased!");
                MessageBox.Show("LEVEL UP!", "Congrats", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Prochain combat (En mode tarpa comme Slay The Spire)
            ShowEncounterSelection();
        }

        private void GameOver()
        {
            MessageBox.Show($"GAME OVER\nYou survived {_battlesWon} battles.", "Womp womp.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ShowMainMenu();
        }
    }
}