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

        // État
        private GameState _currentState;

        // UI
        // Petite update : J'ai ajouté "= null!;" pour dire au compilateur qu'on va les initialiser plus tard
        private Label lblTitle = null!;
        private Button btnStart = null!;
        private Button btnQuit = null!;
        
        // UI de Sélection de classe
        private GroupBox grpClassSelect = null!;
        private Button btnFighter = null!;
        private Button btnWhiteMage = null!;
        private Button btnDarkMage = null!;

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
            this.BackColor = Color.Black; 

            InitializeComponents();
            ShowMainMenu();
        }

        // Initialisation des widgets
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

            // 3. Scène de Combat 
            pnlBattleScene = new Panel();
            pnlBattleScene.Size = new Size(760, 300);
            pnlBattleScene.Location = new Point(12, 12);
            pnlBattleScene.BackColor = Color.Black;
            pnlBattleScene.BorderStyle = BorderStyle.Fixed3D;
            pnlBattleScene.Visible = false;
            this.Controls.Add(pnlBattleScene);

            // Cubic time
            lblEnemySprite = new Label { BackColor = Color.IndianRed, Size = new Size(100, 100), Location = new Point(600, 50), Text = "ENEMY", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblEnemySprite);

            lblPlayerSprite = new Label { BackColor = Color.SteelBlue, Size = new Size(100, 100), Location = new Point(100, 150), Text = "YOU", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblPlayerSprite);

            // Stats Affichées
            lblEnemyStats = new Label { Text = "Goblin Lvl 1\nHP: 20/20", AutoSize = true, Location = new Point(400, 50), Font = new Font("Consolas", 12), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblEnemyStats);

            lblPlayerStats = new Label { Text = "Hero Lvl 1\nHP: 50/50\nMP: 10/10", AutoSize = true, Location = new Point(220, 200), Font = new Font("Consolas", 12), ForeColor = Color.White };
            pnlBattleScene.Controls.Add(lblPlayerStats);

            // 4. Zone de Log (Texte en bas)
            txtGameLog = new RichTextBox();
            txtGameLog.Location = new Point(12, 320);
            txtGameLog.Size = new Size(400, 230);
            txtGameLog.ReadOnly = true;
            txtGameLog.BackColor = Color.Black;
            txtGameLog.ForeColor = Color.White;
            txtGameLog.Font = new Font("Consolas", 10);
            txtGameLog.Visible = false;
            this.Controls.Add(txtGameLog);

            // 5. Menu d'action (Les 4 boutons smashables)
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

        // Le helper pour les boutons de classe (Oui j'ai besoin d'aide moi avec)
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

        // Différents écrans

        private void ShowMainMenu()
        {
            _currentState = GameState.MainMenu;
            lblTitle.Visible = true;
            btnStart.Visible = true;
            btnQuit.Visible = true;
            grpClassSelect.Visible = false;
            HideBattleUI();
        }

        private void ShowClassSelection()
        {
            _currentState = GameState.ClassSelection;
            btnStart.Visible = false;
            btnQuit.Visible = false;
            grpClassSelect.Visible = true;
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
            pnlBattleScene.Visible = true;
            txtGameLog.Visible = true;
            grpActions.Visible = true;
        }

        // LOGIQUE DU JEU (Part 2)

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
                    _playerMaxHP = 100; _playerMaxMana = 20; break;
                case PlayerClass.WhiteMage:
                    _playerMaxHP = 70; _playerMaxMana = 80; break;
                case PlayerClass.DarkMage:
                    _playerMaxHP = 60; _playerMaxMana = 100; break;
                default: _playerMaxHP = 80; _playerMaxMana = 50; break;
            }
            _playerHP = _playerMaxHP;
            _playerMana = _playerMaxMana;

            Log("You chose " + _currentClass.ToString() + "!");
            StartBattle();
        }

        private void StartBattle()
        {
            _currentState = GameState.Battle;
            ShowBattleUI();

            // Génération d'ennemi (De plus en plus fort)
            int difficulty = _battlesWon + 1;
            _enemyMaxHP = 30 + (difficulty * 10);
            _enemyHP = _enemyMaxHP;
            _enemyDamage = 5 + difficulty;
            _enemyName = difficulty % 3 == 0 ? "BIG BOSS ORC" : "Goblin"; // Big Baddie tous les 3 niveaux

            // Mise à jour visuelle (changement de couleur si boss)
            lblEnemySprite.BackColor = difficulty % 3 == 0 ? Color.DarkRed : Color.IndianRed;
            lblEnemySprite.Size = difficulty % 3 == 0 ? new Size(150, 150) : new Size(100, 100);

            UpdateStatsUI();
            Log($"--- BATTLE {_battlesWon + 1} START ---");
            Log($"A wild {_enemyName} appears!");
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
                    int baseAtk = _currentClass == PlayerClass.Fighter ? 15 : 8;
                    damageDealt = baseAtk + (_playerLevel * 2);
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
                            _enemyHP -= spellDmg;
                            Log($"You cast FIREBALL! Dealt {spellDmg} damage.");
                        }
                        else // Fighter spell (faible)
                        {
                            int spellDmg = 15;
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
                // Victoire HELL YEAHHH
                _enemyHP = 0;
                UpdateStatsUI();
                Log($"You defeated the {_enemyName}!");
                WinBattle();
            }
            else
            {
                // Tour de l'ennemi (délai sinon c'est weird)
                EnemyTurn(isBlocking);
            }
        }

        private async void EnemyTurn(bool playerBlocking)
        {
            // Petit délai pour le réalisme
            grpActions.Enabled = false; // Empêche de cliquer
            await Task.Delay(1000);

            int dmg = _enemyDamage;
            if (playerBlocking) dmg /= 2; // Dégâts réduits de moitié si Bloque

            _playerHP -= dmg;
            Log($"The {_enemyName} attacks! You take {dmg} damage.");
            
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
            
            // Level Up Check (Tous les 2 combats pour tester vite)
            int levelUpThreshold = 2; 
            
            MessageBox.Show($"Victory! You won battle #{_battlesWon}.", "Victory", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (_battlesWon % levelUpThreshold == 0)
            {
                _playerLevel++;
                _playerMaxHP += 10;
                _playerMaxMana += 5;
                _playerHP = _playerMaxHP; // Soin complet au level up
                _playerMana = _playerMaxMana;
                Log($"LEVEL UP! You are now level {_playerLevel}. Stats increased!");
                MessageBox.Show("LEVEL UP!", "Congrats", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            // Prochain combat (En mode tarpa comme Slay The Spire)
            StartBattle();
        }

        private void GameOver()
        {
            MessageBox.Show($"GAME OVER\nYou survived {_battlesWon} battles.", "Womp womp.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ShowMainMenu();
        }
    }
}