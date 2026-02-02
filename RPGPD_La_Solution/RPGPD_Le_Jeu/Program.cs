using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq; 

namespace RPGPD_Le_Jeu
{
    // ==========================================
    // CRÉATION DE LA FENÊTRE DU JEU
    // ==========================================
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GameForm());
        }
    }

    // ==========================================
    // DÉFINITIONS & ÉNUMÉRATIONS
    // ==========================================
    public enum PlayerClass
    {
        Fighter,
        WhiteMage,
        DarkMage
    }

    public enum GameState
    {
        MainMenu,
        ClassSelection,
        EncounterSelect,
        Battle,
        Victory,
        GameOver,
        Credits
    }

    // ==========================================
    // TOUTES LES CLASSES VISUELLES
    // ==========================================
    public class Particle
    {
        public PointF Position;
        public PointF Velocity;
        public Color Color;
        public float Size;
        public int Life;
    }

    public class PopupText
    {
        public required string Text;
        public PointF Position;
        public Color Color;
        public float Scale;
        public int Life;
        public float Alpha;
    }

    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }

    // ==========================================
    // LA CLASSE PRINCIPALE
    // ==========================================
    public class GameForm : Form
    {

        // Données du Joueur (Ce que Tom a demandé)
        private PlayerClass _currentClass;
        private int _playerHP;
        private int _playerMaxHP;
        private int _playerMana;
        private int _playerMaxMana;
        private int _playerLevel;
        private int _battlesWon;
        private int _potions;
        private TaskCompletionSource<int>? _inputSignal;
        private int _lastActionSelected = 0; // 1=Atk, 2=Block, 3=Spell, 4=Item (GNIAAAAA)

        // Données de l'Ennemi
        private string _enemyName = "Goblin";
        private int _enemyHP;
        private int _enemyMaxHP;
        private int _enemyDamage;
        
        // Logique Mobs.cs (Tous les IDs)
        private int _leftMobId;
        private int _rightMobId;
        private int _selectedMobId;
        private string _leftEnemyType = "";
        private string _rightEnemyType = "";

        // Variable pour savoir si l'ennemi bloque venant de Mobs.cs
        private bool _enemyIsBlocking;

        // État Global
        private GameState _currentState;

        private System.Windows.Forms.Timer _gameTimer = null!;
        private float _globalTime = 0;
        private Random _rng = new Random();

        // Particules & Popups
        private List<Particle> _particles = new List<Particle>();
        private List<PopupText> _popups = new List<PopupText>(); 
        private Queue<PopupText> _popupQueue = new Queue<PopupText>();

        // Effets d'écran
        private int _shakeDuration = 0;
        private int _shakeMagnitude = 0;
        private int _flashDuration = 0;

        // Offsets d'animation
        private float _playerAnimOffset = 0;
        private float _enemyAnimOffset = 0;

        // Labels & Boutons
        private Label lblTitle = null!;
        private Button btnStart = null!;
        private Button btnQuit = null!;
        private Button btnCredits = null!; 
        
        // Conteneurs
        private GroupBox grpClassSelect = null!;
        private Panel pnlEncounterSelect = null!; 
        private BufferedPanel pnlBattleScene = null!;    // Le terrain du jeu
        private Panel pnlBottomUI = null!;               // Panel du bas (Log/Action)
        private BufferedPanel pnlWinScreen = null!;      // Overlay Victoire/Défaite
        private BufferedPanel pnlCreditsOverlay = null!; // Overlay Crédits
        private GroupBox grpActions = null!;

        // Boutons Dynamiques
        private Button btnFighter = null!;
        private Button btnWhiteMage = null!;
        private Button btnDarkMage = null!;
        private Button btnPathLeft = null!;
        private Button btnPathRight = null!;
        
        private Button btnAttack = null!;
        private Button btnBlock = null!;
        private Button btnSpell = null!;
        private Button btnItem = null!;
        
        private Button btnWinReturn = null!;
        private Button btnCreditsClose = null!;

        // Labels Informatifs
        private Label lblPlayerStats = null!;
        private Label lblEnemyStats = null!;
        private Label lblWinMessage = null!;
        private Label lblCreditsText = null!;
        private RichTextBox txtGameLog = null!;

        // ==========================================
        //              INITIALISATION
        // ==========================================
        public GameForm()
        {
            SetupWindow();
            InitializeComponents();
            InitializeAnimation(); 
            ShowMainMenu();
        }

        private void SetupWindow()
        {
            this.Text = "RudacoPG - The Adventure";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black; 
            
            // Configuration pour capturer Échap
            this.KeyPreview = true;
            this.KeyDown += HandleGlobalInput;

            // Anti-Flickering Global
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void InitializeAnimation()
        {
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 16; // Cible d'environ 60 FPS
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        // ==========================================
        // BOUCLE DE JEU (GAME LOOP)
        // ==========================================
        private void GameLoop(object? sender, EventArgs e)
        {
            _globalTime += 0.1f;

            // 1. Mise à jour des Timers d'effets
            if (_shakeDuration > 0) _shakeDuration--;
            if (_flashDuration > 0) _flashDuration--;

            // 2. Physique des Particules
            UpdateParticles();

            // 3. Gestion des Popups (Avec la grosse queue)
            UpdatePopups();

            // 4. Animation des Sprites
            _playerAnimOffset = Lerp(_playerAnimOffset, 0, 0.1f);
            _enemyAnimOffset = Lerp(_enemyAnimOffset, 0, 0.1f);

            // 5. Redessin des écrans actifs
            if (_currentState == GameState.Battle) pnlBattleScene.Invalidate();
            if (_currentState == GameState.Victory || _currentState == GameState.GameOver) pnlWinScreen.Invalidate();
            
            // 6. Animation du Menu Principal
            if (_currentState == GameState.MainMenu)
            {
                AnimateMainMenuTitle();
            }
        }

        private void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                Particle p = _particles[i];
                p.Position.X += p.Velocity.X;
                p.Position.Y += p.Velocity.Y;
                p.Life--;
                p.Size *= 0.95f; 
                if (p.Life <= 0) _particles.RemoveAt(i);
            }
        }

        private void UpdatePopups()
        {
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                PopupText p = _popups[i];
                p.Position.Y -= 1.0f; // Float up
                p.Life--;
                
                // Calcul Alpha sécurisé
                float rawAlpha = p.Life / 60.0f;
                p.Alpha = Math.Min(1.0f, Math.Max(0.0f, rawAlpha)); 

                if (p.Scale < 1.0f) p.Scale += 0.1f;
                if (p.Life <= 0) _popups.RemoveAt(i);
            }

            // Si aucun popup actif, on affiche le prochain
            if (_popups.Count == 0 && _popupQueue.Count > 0)
            {
                _popups.Add(_popupQueue.Dequeue());
            }
        }

        private void AnimateMainMenuTitle()
        {
            float scale = 1.0f + (float)Math.Sin(_globalTime * 2) * 0.05f;
            lblTitle.Font = new Font(lblTitle.Font.FontFamily, 72 * scale, FontStyle.Italic);
        }

        // ==========================================
        // GESTION DES ENTRÉES (INPUT)
        // ==========================================
        private void HandleGlobalInput(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ResetGameData();
                ShowMainMenu();
            }
        }

        private void ToggleInput(bool enabled)
        {
            grpActions.Enabled = enabled;
        }

        // ==========================================
        // LOGIQUE DU JEU (CORE)
        // ==========================================

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
            ShowEncounterSelection();
        }

        private void InitializeBattle(string enemyType)
        {
            _currentState = GameState.Battle;
            ShowBattleUI();

            _enemyName = enemyType;
            _enemyIsBlocking = false; 
            ToggleInput(true); // Réactiver les boutons

            // Configuration difficulté
            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;
            
            // Génération via Mobs.cs
            Mobs mobGenerator = new Mobs(currentDiff);
            _enemyMaxHP = mobGenerator.Générer_HP(currentDiff, _selectedMobId);
            _enemyDamage = mobGenerator.Générer_Attaque(currentDiff, _selectedMobId);
            _enemyHP = _enemyMaxHP;

            UpdateStatsUI();
            Log($"--- BATTLE {_battlesWon + 1} START ---");
            Log($"You encounter a {_enemyName}!");
        }

        // --- TOUR DU JOUEUR ---
        private async void PlayerTurn(string action)
        {
            if (_currentState != GameState.Battle) return;

            switch (action)
            {
                case "Attack": _lastActionSelected = 1; break;
                case "Block":  _lastActionSelected = 2; break;
                case "Spell":  _lastActionSelected = 3; break;
                case "Item":   _lastActionSelected = 4; break;
            }

            ToggleInput(false); // Bloque l'input immédiatement

            // Animation visuelle
            if(action == "Attack") _playerAnimOffset = 200; 
            await Task.Delay(300); // Délai de l'impact

            bool turnComplete = true;

            switch (action)
            {
                case "Attack": PerformAttack(); break;
                case "Block": PerformBlock(); break;
                case "Spell": turnComplete = PerformSpell(); break; // Peut échouer si pas de mana
                case "Item": turnComplete = PerformItem(); break;   // Peut échouer si pas de potion
            }

            if (turnComplete)
            {
                UpdateStatsUI();
                CheckBattleStatus();
            }
            else
            {
                // Si l'action a échoué (pas de mana), on réactive les boutons
                ToggleInput(true);
            }
        }

        private void PerformAttack()
        {
            int damageDealt;
            Random rnd = new Random();

            // Calcul dégâts de base
            if (_currentClass == PlayerClass.Fighter) damageDealt = rnd.Next(3, 7);
            else damageDealt = rnd.Next(2, 5);

            // Gestion Critique
            if (rnd.Next(0, 100) < 4) // 4%
            {
                damageDealt *= 2;
                ShakeScreen(15, 10);
                SpawnPopup("CRITICAL!", Color.Yellow, 36);
                Log("CRITICAL HIT! You hit a weak spot!");
            }
            else ShakeScreen(5, 5);
            
            SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Red, 20);

            // Gestion Blocage Ennemi (Mobs.cs)
            HandleEnemyBlockInteraction(ref damageDealt);

            _enemyHP -= damageDealt;
            Log($"You attacked for {damageDealt} damage!");
        }

        private void HandleEnemyBlockInteraction(ref int damage)
        {
            int currentDiff = _playerLevel > 3 ? 3 : _playerLevel;
            Mobs mobCheck = new Mobs(currentDiff);
            
            // Prévision de l'action ennemie pour vérifier le blocage
            int mobAction = mobCheck.ChoixActionEnnemi(currentDiff, _selectedMobId, _playerHP, (int)_currentClass, _enemyHP);
            
            if (mobAction == 2) // 2 = Block
            {
                damage /= 2;
                int healAmount = (int)(_enemyMaxHP * 0.20);
                _enemyHP = Math.Min(_enemyHP + healAmount, _enemyMaxHP);
                
                Log($"The {_enemyName} BLOCKS! Damage halved & Healed {healAmount} HP.");
                SpawnPopup("BLOCK", Color.Cyan);
                SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Cyan, 10);
            }
        }

        private void PerformBlock()
        {
            _enemyIsBlocking = true;
            Log("You brace yourself for impact (Defense UP)!");
            _playerMana = Math.Min(_playerMana + 5, _playerMaxMana);
        }

        private bool PerformSpell()
        {
            if (_playerMana < 10)
            {
                Log("Not enough Mana!");
                SpawnPopup("NO MANA", Color.Gray);
                return false;
            }

            _playerMana -= 10;

            if (_currentClass == PlayerClass.WhiteMage)
            {
                int heal = 25 + (_playerLevel * 5);
                _playerHP = Math.Min(_playerHP + heal, _playerMaxHP);
                Log($"You cast HEAL! Recovered {heal} HP.");
                SpawnPopup("HEAL", Color.LightGreen);
                SpawnParticles(200, pnlBattleScene.Height/2, Color.Gold, 20);
            }
            else
            {
                // Spells du Dark Mage et Fighter
                int spellDmg = (_currentClass == PlayerClass.DarkMage) ? 25 + (_playerLevel * 5) : 15;
                Color fxColor = (_currentClass == PlayerClass.DarkMage) ? Color.Purple : Color.Orange;
                
                SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, fxColor, 30);
                
                // Block check pour les spells
                if (_enemyIsBlocking) 
                {
                    spellDmg /= 2;
                    Log("Enemy magic resistance UP! Damage halved.");
                }

                _enemyHP -= spellDmg;
                Log($"You cast SPELL! Dealt {spellDmg} damage.");
            }
            return true;
        }

        private bool PerformItem()
        {
            if (_potions <= 0)
            {
                Log("No potions left!");
                SpawnPopup("EMPTY", Color.Gray);
                return false;
            }

            _potions--;
            _playerHP = Math.Min(_playerHP + 50, _playerMaxHP);
            Log("Used a Potion! +50 HP.");
            SpawnPopup("+50 HP", Color.LightGreen);
            SpawnParticles(200, pnlBattleScene.Height/2, Color.LightGreen, 20);
            return true;
        }

        // --- VÉRIFICATION ET TOUR ENNEMI ---
        private void CheckBattleStatus()
        {
            if (_enemyHP <= 0)
            {
                _enemyHP = 0;
                UpdateStatsUI();
                Log($"You defeated the {_enemyName}!");
                SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Gold, 50); 
                WinBattle();
            }
            else
            {
                EnemyTurn();
            }
        }

        private async void EnemyTurn()
        {
            await Task.Delay(1000); // Temps de réflexion

            int currentDiff = _playerLevel > 3 ? 3 : _playerLevel;
            Mobs mobAI = new Mobs(currentDiff);
            int actionCode = mobAI.ChoixActionEnnemi(currentDiff, _selectedMobId, _playerHP, (int)_currentClass, _enemyHP);

            _enemyIsBlocking = false; // Reset

            switch (actionCode)
            {
                case 1: // Attaque
                    await EnemyPerformAttack(false);
                    break;
                case 2: // Block
                    _enemyIsBlocking = true;
                    int regen = 5 + (currentDiff * 2);
                    _enemyHP = Math.Min(_enemyHP + regen, _enemyMaxHP);
                    SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Green, 10);
                    SpawnPopup("REGEN", Color.Green);
                    Log($"The {_enemyName} regenerates {regen} HP!");
                    break;
                case 3: // Spécial
                    await EnemyPerformAttack(true);
                    break;
            }
            
            UpdateStatsUI();

            if (_playerHP <= 0)
            {
                _playerHP = 0;
                UpdateStatsUI();
                GameOver();
            }
            
            ToggleInput(true); // C'est au joueur de jouer
        }

        private async Task EnemyPerformAttack(bool isSpecial)
        {
            _enemyAnimOffset = isSpecial ? -300 : -200; // Animation
            await Task.Delay(200);

            int dmg = isSpecial ? (int)(_enemyDamage * 1.5) : _enemyDamage;
            
            // Critique Ennemi
            if (_rng.Next(0, 100) < 4)
            {
                dmg *= 2;
                ShakeScreen(20, 15);
                SpawnPopup("OUCH!", Color.Red, 36);
                Log("CRITICAL HIT! Enemy smashed you!");
            }
            else ShakeScreen(isSpecial ? 25 : 5, isSpecial ? 20 : 5);

            // Block Joueur (Simulé ici en faisant une division si le joueur a cliqué Block)
            
            _playerHP -= dmg;
            SpawnParticles(200, pnlBattleScene.Height/2, Color.Red, 15);
            Log(isSpecial ? "SPECIAL ABILITY!" : "Enemy attacks!");
        }

        // --- FIN DE COMBAT ---
        private void WinBattle()
        {
            _battlesWon++;

            if (_battlesWon >= 10)
            {
                ShowWinScreen();
                return;
            }
            
            SpawnPopup("VICTORY!", Color.Gold, 48);

            // Level Up Check
            if (_battlesWon % 3 == 0)
            {
                _playerLevel++;
                _playerMaxMana += 5;
                _playerHP = _playerMaxHP; 
                _playerMana = _playerMaxMana;
                Log($"LEVEL UP! You are now level {_playerLevel}.");
                SpawnPopup("LEVEL UP!", Color.Cyan, 60);
                SpawnParticles(200, pnlBattleScene.Height/2, Color.Cyan, 100);
            }

            // Délai avant prochain choix
            Task.Delay(2000).ContinueWith(t => this.Invoke((MethodInvoker)delegate {
                ShowEncounterSelection();
            }));
        }

        private void GameOver()
        {
            _currentState = GameState.GameOver;
            
            // Configuration de l'overlay Game Over
            pnlWinScreen.Visible = true;
            pnlWinScreen.BringToFront();
            
            lblWinMessage.Text = "WOMP WOMP";
            lblWinMessage.ForeColor = Color.DarkRed;
            
            // Délai pour le bouton
            btnWinReturn.Visible = false;
            btnWinReturn.Text = "TRY AGAIN";
            
            Task.Delay(1500).ContinueWith(t => this.Invoke((MethodInvoker)delegate {
                btnWinReturn.Visible = true;
            }));
        }

        // ==========================================
        // GESTION DE L'INTERFACE (UI & NAVIGATION)
        // ==========================================

        private void ShowMainMenu()
        {
            _currentState = GameState.MainMenu;
            lblTitle.Visible = true;
            btnStart.Visible = true;
            btnQuit.Visible = true;
            btnCredits.Visible = true;
            grpClassSelect.Visible = false;
            pnlEncounterSelect.Visible = false;
            pnlWinScreen.Visible = false;
            pnlCreditsOverlay.Visible = false;
            HideBattleUI();
        }

        private void ShowClassSelection()
        {
            _currentState = GameState.ClassSelection;
            btnStart.Visible = false;
            btnQuit.Visible = false;
            btnCredits.Visible = false;
            grpClassSelect.Visible = true;
            lblTitle.Visible = false;
        }

        private void ShowEncounterSelection()
        {
            _currentState = GameState.EncounterSelect;
            grpClassSelect.Visible = false;
            HideBattleUI();
            pnlEncounterSelect.Visible = true;
            pnlWinScreen.Visible = false;
            
            GenerateEncounterChoices();
        }

        private void GenerateEncounterChoices()
        {
            int currentDiff = _playerLevel > 3 ? 3 : _playerLevel;
            Mobs mobGenerator = new Mobs(currentDiff);

            _leftMobId = mobGenerator.Générer_choix_Mob();
            _rightMobId = mobGenerator.Générer_choix_Mob();

            _leftEnemyType = GetMobName(currentDiff, _leftMobId);
            _rightEnemyType = GetMobName(currentDiff, _rightMobId);

            bool leftJammed = _rng.Next(0, 100) < 15;
            bool rightJammed = _rng.Next(0, 100) < 15;

            string leftText = leftJammed ? "?? JAMMED ??" : _leftEnemyType;
            string rightText = rightJammed ? "?? JAMMED ??" : _rightEnemyType;

            btnPathLeft.Text = $"PATH A\n\nStrange Corridor.\n\nSenses detect:\n{leftText}";
            btnPathRight.Text = $"PATH B\n\nDark Corridor...\n\nSenses detect:\n{rightText}";
            
            btnPathLeft.ForeColor = leftJammed ? Color.Gray : Color.White;
            btnPathRight.ForeColor = rightJammed ? Color.Gray : Color.White;
        }

        private void SelectPath(bool isLeft)
        {
            string enemyName = isLeft ? _leftEnemyType : _rightEnemyType;
            _selectedMobId = isLeft ? _leftMobId : _rightMobId;
            InitializeBattle(enemyName);
        }

        private void ShowWinScreen()
        {
            _currentState = GameState.Victory;
            HideBattleUI();
            pnlEncounterSelect.Visible = false;
            pnlWinScreen.Visible = true;
            pnlWinScreen.BringToFront();
            
            lblWinMessage.Text = "VICTORY!";
            lblWinMessage.ForeColor = Color.Gold;
            btnWinReturn.Text = "RETURN TO MAIN MENU";
            btnWinReturn.Visible = true;
        }

        private void ShowCredits() { pnlCreditsOverlay.Visible = true; pnlCreditsOverlay.BringToFront(); }
        private void HideCredits() { pnlCreditsOverlay.Visible = false; }
        private void HideBattleUI() { pnlBattleScene.Visible = false; pnlBottomUI.Visible = false; }
        
        // Gestion de l'overlapping
        private void ShowBattleUI() 
        { 
            lblTitle.Visible = false;
            grpClassSelect.Visible = false;
            pnlEncounterSelect.Visible = false;
            pnlWinScreen.Visible = false;
            pnlCreditsOverlay.Visible = false;

            pnlBattleScene.Visible = true; 
            pnlBottomUI.Visible = true; 
        }

        private void ResetGameData()
        {
            _particles.Clear();
            _popups.Clear();
            _popupQueue.Clear();
            _shakeDuration = 0;
        }

        // ==========================================
        // RENDU GRAPHIQUE (DESSINS)
        // ==========================================

        private void BattleScene_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pnlBattleScene.Width;
            int h = pnlBattleScene.Height;

            DrawBackground(g, w, h);
            ApplyShake(g);
            
            // Dessins des entités
            DrawPlayer(g, w, h);
            DrawMob(g, w, h, _enemyName);
            
            // Dessins des effets
            DrawParticles(g);
            DrawPopups(g);
            DrawFlash(g, w, h);
        }

        private void WinScreen_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int w = pnlWinScreen.Width;
            int h = pnlWinScreen.Height;

            if (_currentState == GameState.Victory)
            {
                // Particules dorées
                for(int i=0; i<100; i++)
                {
                    int x = _rng.Next(w);
                    int y = (int)((_rng.Next(h) + _globalTime * 5) % h);
                    g.FillEllipse(Brushes.Gold, x, y, 5, 5);
                }
            }
            else if (_currentState == GameState.GameOver)
            {
                // Pluie de la terreur terorisante
                for(int i=0; i<100; i++)
                {
                    int x = _rng.Next(w);
                    int y = (int)((_rng.Next(h) + _globalTime * 15) % h);
                    g.FillRectangle(Brushes.DarkRed, x, y, 2, 10);
                }
            }
        }

        // Helpers de dessin
        private void DrawBackground(Graphics g, int w, int h)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(new Point(0,0), new Point(w, h), Color.FromArgb(20,0,20), Color.Black))
            {
                g.FillRectangle(brush, 0, 0, w, h);
            }
            // Étoiles
            Random r = new Random(1234); 
            for(int i=0; i<50; i++)
            {
                int x = r.Next(w);
                int y = r.Next(h);
                int size = r.Next(1, 4);
                float alpha = (float)(Math.Sin(_globalTime * 0.1f + i) + 1) / 2 * 255;
                using(SolidBrush b = new SolidBrush(Color.FromArgb((int)alpha, 255, 255, 255)))
                {
                    g.FillEllipse(b, x, y, size, size);
                }
            }
        }

        private void ApplyShake(Graphics g)
        {
            if(_shakeDuration > 0)
            {
                int shakeX = _rng.Next(-_shakeMagnitude, _shakeMagnitude);
                int shakeY = _rng.Next(-_shakeMagnitude, _shakeMagnitude);
                g.TranslateTransform(shakeX, shakeY);
            }
        }

        private void DrawPlayer(Graphics g, int w, int h)
        {
            float breathe = (float)Math.Sin(_globalTime * 0.2f) * 5;
            float x = 200 + _playerAnimOffset;
            float y = h/2 + breathe;

            using(SolidBrush b = new SolidBrush(Color.SteelBlue))
                g.FillEllipse(b, x, y, 80, 80);
            
            using(SolidBrush b = new SolidBrush(Color.PeachPuff))
                g.FillEllipse(b, x+15, y-30, 50, 50);

            // Accessoires
            if (_currentClass == PlayerClass.Fighter)
            {
                using(Pen p = new Pen(Color.Silver, 5)) g.DrawLine(p, x+60, y+40, x+100, y+10);
            }
            else if (_currentClass == PlayerClass.DarkMage)
            {
                PointF[] hat = { new PointF(x+10, y-30), new PointF(x+70, y-30), new PointF(x+40, y-80) };
                g.FillPolygon(Brushes.Purple, hat);
            }
            else // Mage Blanc
            {
                using(Pen p = new Pen(Color.Gold, 3)) g.DrawEllipse(p, x+15, y-50, 50, 10);
            }

            // Clignement des yeux
            if ((int)(_globalTime * 10) % 50 > 2)
            {
                g.FillEllipse(Brushes.Black, x+30, y-15, 5, 5);
                g.FillEllipse(Brushes.Black, x+50, y-15, 5, 5);
            }
        }

        private void DrawMob(Graphics g, int w, int h, string name)
        {
            float breathe = (float)Math.Sin(_globalTime * 0.15f + 2) * 8;
            float x = w - 300 + _enemyAnimOffset;
            float y = h/2 - 50 + breathe;

            if (name.Contains("Goblin") || name.Contains("Gobelin"))
            {
                g.FillEllipse(Brushes.ForestGreen, x, y, 100, 100);
                PointF[] lEar = { new PointF(x, y+30), new PointF(x-30, y+10), new PointF(x+10, y+40) };
                PointF[] rEar = { new PointF(x+100, y+30), new PointF(x+130, y+10), new PointF(x+90, y+40) };
                g.FillPolygon(Brushes.ForestGreen, lEar);
                g.FillPolygon(Brushes.ForestGreen, rEar);
                g.FillEllipse(Brushes.Red, x+25, y+30, 15, 15);
                g.FillEllipse(Brushes.Red, x+60, y+30, 15, 15);
            }
            else if (name.Contains("Boss") || name.Contains("Troll") || name.Contains("Orc"))
            {
                g.FillRectangle(Brushes.DarkRed, x, y, 150, 150);
                g.FillPolygon(Brushes.Gray, new PointF[] { new PointF(x+20, y), new PointF(x+30, y-40), new PointF(x+40, y) });
                g.FillPolygon(Brushes.Gray, new PointF[] { new PointF(x+110, y), new PointF(x+120, y-40), new PointF(x+130, y) });
                g.FillRectangle(Brushes.Black, x+30, y+50, 30, 20);
                g.FillRectangle(Brushes.Black, x+90, y+50, 30, 20);
                g.DrawArc(Pens.Black, x+40, y+100, 70, 20, 180, 180); 
            }
            else if (name.Contains("Squelette") || name.Contains("Skeleton"))
            {
                g.FillEllipse(Brushes.WhiteSmoke, x+30, y, 60, 60); 
                g.FillRectangle(Brushes.WhiteSmoke, x+55, y+60, 10, 60); 
                g.FillRectangle(Brushes.WhiteSmoke, x+30, y+70, 60, 10); 
                g.FillEllipse(Brushes.Black, x+45, y+20, 10, 10);
                g.FillEllipse(Brushes.Black, x+65, y+20, 10, 10);
            }
            else if (name.Contains("Slime") || name.Contains("Rat"))
            {
                g.FillPie(Brushes.DodgerBlue, x, y+20, 100, 80, 0, -180);
                g.FillRectangle(Brushes.DodgerBlue, x, y+60, 100, 40);
                g.FillEllipse(Brushes.White, x+30, y+50, 15, 15);
                g.FillEllipse(Brushes.White, x+60, y+50, 15, 15);
                g.FillEllipse(Brushes.Black, x+35, y+55, 5, 5);
                g.FillEllipse(Brushes.Black, x+65, y+55, 5, 5);
            }
            else 
            {
                g.FillEllipse(Brushes.DarkGray, x, y, 100, 100);
                g.DrawString("?", new Font("Arial", 50), Brushes.Red, x+20, y+20);
            }
            
            if(_enemyIsBlocking)
            {
                using(Pen p = new Pen(Color.Cyan, 5))
                    g.DrawEllipse(p, x-10, y-10, 150, 150);
            }
        }

        private void DrawParticles(Graphics g)
        {
            foreach (var p in _particles)
            {
                int alpha = Math.Min(255, Math.Max(0, (int)(p.Life * 8.5)));
                using(SolidBrush b = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                {
                    g.FillEllipse(b, p.Position.X, p.Position.Y, p.Size, p.Size);
                }
            }
        }

        private void DrawPopups(Graphics g)
        {
            foreach(var p in _popups)
            {
                int alpha = Math.Min(255, Math.Max(0, (int)(p.Alpha * 255)));
                using(Font font = new Font("Impact", 60 * p.Scale, FontStyle.Bold))
                using(Brush b = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                {
                    SizeF size = g.MeasureString(p.Text, font);
                    g.DrawString(p.Text, font, b, p.Position.X - size.Width/2, p.Position.Y - size.Height/2);
                }
            }
        }

        private void DrawFlash(Graphics g, int w, int h)
        {
            if(_flashDuration > 0)
            {
                using(SolidBrush b = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
                {
                    g.FillRectangle(b, 0, 0, w, h);
                }
            }
        }

        // ==========================================
        // HELPERS LOGIQUE (MATHS & MOBS)
        // ==========================================

        private float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        private void ShakeScreen(int magnitude, int duration)
        {
            _shakeMagnitude = magnitude;
            _shakeDuration = duration;
        }

        private void SpawnParticles(float x, float y, Color c, int count)
        {
            for(int i=0; i<count; i++)
            {
                _particles.Add(new Particle() {
                    Position = new PointF(x, y),
                    Velocity = new PointF(_rng.Next(-10, 11), _rng.Next(-10, 11)),
                    Color = c,
                    Size = _rng.Next(5, 15),
                    Life = 30
                });
            }
        }

        private void SpawnPopup(string text, Color c, int size = 24)
        {
            _popupQueue.Enqueue(new PopupText()
            {
                Text = text,
                Position = new PointF(pnlBattleScene.Width / 2, pnlBattleScene.Height / 2),
                Color = c,
                Scale = 0.1f,
                Life = 80, 
                Alpha = 1.0f
            });
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

        private string GetMobName(int difficulty, int choix)
        {
            // J'ai un peu répliqué la logique pour nommer basée sur Mobs.cs
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

        // ==========================================
        // INITIALISATION DES COMPOSANTS (UI)
        // ==========================================
        private void InitializeComponents()
        {
            // Utilisation des bordures d'écran en fullscreen
            Rectangle screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            int w = screenBounds.Width;
            int h = screenBounds.Height;
            int cx = w / 2;
            int cy = h / 2;

            // 1. Titre
            lblTitle = new Label();
            lblTitle.Text = "RudacoPG";
            lblTitle.Font = new Font("Impact", 72, FontStyle.Italic); 
            lblTitle.AutoSize = false; 
            lblTitle.Size = new Size(w, 200); 
            lblTitle.TextAlign = ContentAlignment.MiddleCenter; 
            lblTitle.ForeColor = Color.Cyan;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Location = new Point(0, cy - 250);
            this.Controls.Add(lblTitle);

            // 2. Boutons du menu
            btnStart = CreateMenuButton("PLAY", cx - 150, cy, (s, e) => ShowClassSelection());
            btnCredits = CreateMenuButton("CREDITS", cx - 150, cy + 80, (s, e) => ShowCredits());
            btnQuit = CreateMenuButton("QUIT", cx - 150, cy + 160, (s, e) => Application.Exit());
            
            // 3. Crédits
            SetupCreditsOverlay(cx, cy);

            // 4. Sélection de classe
            SetupClassSelection(cx, cy);

            // 5. Sélection du mob
            SetupEncounterSelect(w, h);

            // 6. Scène de combat
            SetupBattleScene(w, h);

            // 7. Victoire/Défaite
            SetupWinScreen(w, h, cx, cy);
        }

        private void SetupCreditsOverlay(int cx, int cy)
        {
            pnlCreditsOverlay = new BufferedPanel();
            pnlCreditsOverlay.Size = new Size(600, 400);
            pnlCreditsOverlay.Location = new Point(cx - 300, cy - 200);
            pnlCreditsOverlay.BackColor = Color.FromArgb(240, 20, 20, 20); 
            pnlCreditsOverlay.Visible = false;
            pnlCreditsOverlay.Paint += (s, e) => e.Graphics.DrawRectangle(Pens.Cyan, 0,0, 599, 399);
            this.Controls.Add(pnlCreditsOverlay);

            lblCreditsText = new Label();
            lblCreditsText.Text = "Made with LOVE by\n\nFélix Lehoux\n&\nThomas Rudacovitch\n\nFor the Ultimate RPG Experience.";
            lblCreditsText.Font = new Font("Arial", 18, FontStyle.Bold);
            lblCreditsText.ForeColor = Color.White;
            lblCreditsText.AutoSize = false;
            lblCreditsText.Size = new Size(600, 300);
            lblCreditsText.TextAlign = ContentAlignment.MiddleCenter;
            lblCreditsText.Location = new Point(0, 20);
            pnlCreditsOverlay.Controls.Add(lblCreditsText);

            btnCreditsClose = CreateMenuButton("CLOSE", 150, 320, (s, e) => HideCredits());
            pnlCreditsOverlay.Controls.Add(btnCreditsClose);
        }

        private void SetupClassSelection(int cx, int cy)
        {
            grpClassSelect = new GroupBox();
            grpClassSelect.Text = "CHOOSE YOUR DESTINY";
            grpClassSelect.Font = new Font("Arial", 16, FontStyle.Bold);
            grpClassSelect.ForeColor = Color.White;
            grpClassSelect.Size = new Size(600, 400);
            grpClassSelect.Location = new Point(cx - 300, cy - 200);
            grpClassSelect.Visible = false;
            grpClassSelect.BackColor = Color.FromArgb(20, 20, 20);
            
            btnFighter = CreateClassButton("FIGHTER\n(High HP/Atk)", 80, Color.DarkRed);
            btnFighter.Click += (s, e) => StartGame(PlayerClass.Fighter);
            
            btnWhiteMage = CreateClassButton("WHITE MAGE\n(Heal/Def)", 160, Color.LightBlue);
            btnWhiteMage.Click += (s, e) => StartGame(PlayerClass.WhiteMage);

            btnDarkMage = CreateClassButton("DARK MAGE\n(High Dmg Spell)", 240, Color.Purple);
            btnDarkMage.Click += (s, e) => StartGame(PlayerClass.DarkMage);

            grpClassSelect.Controls.Add(btnFighter);
            grpClassSelect.Controls.Add(btnWhiteMage);
            grpClassSelect.Controls.Add(btnDarkMage);
            this.Controls.Add(grpClassSelect);
        }

        private void SetupEncounterSelect(int w, int h)
        {
            pnlEncounterSelect = new Panel();
            pnlEncounterSelect.Dock = DockStyle.Fill; 
            pnlEncounterSelect.BackColor = Color.Black;
            pnlEncounterSelect.Visible = false;
            this.Controls.Add(pnlEncounterSelect);

            btnPathLeft = CreatePathButton(0, 0, w/2, h);
            btnPathLeft.Click += (s, e) => SelectPath(true);
            pnlEncounterSelect.Controls.Add(btnPathLeft);

            btnPathRight = CreatePathButton(w/2, 0, w/2, h);
            btnPathRight.Click += (s, e) => SelectPath(false);
            pnlEncounterSelect.Controls.Add(btnPathRight);
        }

        private void SetupBattleScene(int w, int h)
        {
            // Je crée le pannel du bas en premier
            pnlBottomUI = new Panel();
            pnlBottomUI.Dock = DockStyle.Bottom;
            pnlBottomUI.Height = 250;
            pnlBottomUI.BackColor = Color.Transparent; // On montre le background noir
            this.Controls.Add(pnlBottomUI);

            // Système de log des événements (Console)
            txtGameLog = new RichTextBox();
            txtGameLog.Location = new Point(20, 20);
            txtGameLog.Size = new Size(w/2 - 40, 210);
            txtGameLog.ReadOnly = true;
            txtGameLog.BackColor = Color.FromArgb(10, 10, 10);
            txtGameLog.ForeColor = Color.LightGray;
            txtGameLog.Font = new Font("Consolas", 12);
            txtGameLog.BorderStyle = BorderStyle.None;
            pnlBottomUI.Controls.Add(txtGameLog);

            // Actions à l'intérieur du pannel
            grpActions = new GroupBox();
            grpActions.Text = "";
            grpActions.Location = new Point(w/2, 10);
            grpActions.Size = new Size(w/2 - 20, 230);
            grpActions.BackColor = Color.Transparent;
            pnlBottomUI.Controls.Add(grpActions);

            int bx = 20; int by = 20; int bw = (grpActions.Width / 2) - 40; int bh = 80;

            btnAttack = CreateActionButton("ATTACK", bx, by, bw, bh, Color.DarkRed);
            btnAttack.Click += (s, e) => PlayerTurn("Attack");

            btnBlock = CreateActionButton("BLOCK", bx + bw + 20, by, bw, bh, Color.DarkBlue);
            btnBlock.Click += (s, e) => PlayerTurn("Block");

            btnSpell = CreateActionButton("SPELL", bx, by + bh + 20, bw, bh, Color.Purple);
            btnSpell.Click += (s, e) => PlayerTurn("Spell");

            btnItem = CreateActionButton("ITEM", bx + bw + 20, by + bh + 20, bw, bh, Color.DarkGreen);
            btnItem.Click += (s, e) => PlayerTurn("Item");

            grpActions.Controls.AddRange(new Control[] { btnAttack, btnBlock, btnSpell, btnItem });

            // Créer la scène de bataille
            pnlBattleScene = new BufferedPanel();
            pnlBattleScene.Dock = DockStyle.Fill; 
            pnlBattleScene.BackColor = Color.Black;
            pnlBattleScene.Visible = false;
            pnlBattleScene.Paint += BattleScene_Paint; 
            this.Controls.Add(pnlBattleScene); // Ajoute le dernier pour remplir l'espace du dessous

            lblEnemyStats = new Label { Text = "", AutoSize = true, Location = new Point(w - 300, 50), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Red, BackColor = Color.Transparent };
            pnlBattleScene.Controls.Add(lblEnemyStats);

            lblPlayerStats = new Label { Text = "", AutoSize = true, Location = new Point(50, 50), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Cyan, BackColor = Color.Transparent };
            pnlBattleScene.Controls.Add(lblPlayerStats);
        }

        private void SetupWinScreen(int w, int h, int cx, int cy)
        {
            pnlWinScreen = new BufferedPanel();
            pnlWinScreen.Dock = DockStyle.Fill;
            pnlWinScreen.BackColor = Color.Black;
            pnlWinScreen.Visible = false;
            pnlWinScreen.Paint += WinScreen_Paint;
            this.Controls.Add(pnlWinScreen);
            pnlWinScreen.BringToFront(); // On s'assure que se soit sur le dessus

            lblWinMessage = new Label();
            lblWinMessage.Text = "VICTORY!";
            lblWinMessage.Font = new Font("Impact", 72, FontStyle.Italic);
            lblWinMessage.ForeColor = Color.Gold;
            lblWinMessage.AutoSize = false;
            lblWinMessage.Size = new Size(w, 300);
            lblWinMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblWinMessage.Location = new Point(0, cy - 200);
            lblWinMessage.BackColor = Color.Transparent;
            pnlWinScreen.Controls.Add(lblWinMessage);

            btnWinReturn = CreateMenuButton("RETURN TO MAIN MENU", cx - 150, cy + 150, (s, e) => ShowMainMenu());
            pnlWinScreen.Controls.Add(btnWinReturn);
        }

        // --- Création des boutons ---

        private Button CreateMenuButton(string text, int x, int y, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(300, 70);
            btn.Font = new Font("Arial", 18, FontStyle.Bold);
            btn.BackColor = Color.FromArgb(40, 40, 40);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.Cyan;
            btn.FlatAppearance.BorderSize = 2;
            
            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(60,60,60); btn.Size = new Size(320, 80); btn.Location = new Point(x-10, y-5); };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(40,40,40); btn.Size = new Size(300, 70); btn.Location = new Point(x, y); };
            
            btn.Click += onClick;
            this.Controls.Add(btn); // On ajoute au parent
            return btn;
        }

        private Button CreateClassButton(string text, int y, Color c)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(100, y);
            btn.Size = new Size(400, 70);
            btn.BackColor = c;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.White;
            btn.Font = new Font("Arial", 14, FontStyle.Bold);
            return btn;
        }

        private Button CreateActionButton(string text, int x, int y, int w, int h, Color c)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.Font = new Font("Arial", 16, FontStyle.Bold);
            btn.BackColor = c;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.White;
            
            btn.MouseEnter += (s, e) => btn.FlatAppearance.BorderColor = Color.Yellow;
            btn.MouseLeave += (s, e) => btn.FlatAppearance.BorderColor = Color.White;
            return btn;
        }

        private Button CreatePathButton(int x, int y, int w, int h)
        {
            Button btn = new Button();
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(20, 20, 20);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Courier New", 24, FontStyle.Bold);
            
            btn.MouseEnter += (s,e) => btn.BackColor = Color.FromArgb(40,40,40);
            btn.MouseLeave += (s,e) => btn.BackColor = Color.FromArgb(20,20,20);
            return btn;
        }

        //
        //
        //
        // Alerte remaniement du combat par Thomas, Toxique pas touché

        private void BattleParThomas(int difficulty, int playerClass, int choixDeLennemi)
        {
            Mobs mob = new Mobs(difficulty);
            Player player = new Player(playerClass);
            int EnnemiHP = mob.Générer_HP(difficulty, choixDeLennemi);
            int playerHP = 0;
            int playerMP = 0;
            int playerPotion = 0;
            int playerAction = 0;
            Random rand = new Random();
            int intDivinDuJoueur = 0;
            int intDivinDuMobs = 0;
            int EnnemiAction = 0;
            

            do
            {
                // Tour du joueur
                playerAction = rand.Next(1, 5); // 1-Attack, 2-Block, 3-Spell, 4-Item
                // Fin tour du joueur
                
                switch (playerAction)
                {
                    case 1: // Attack
                        EnnemiAction = mob.ChoixActionEnnemi(difficulty, choixDeLennemi, playerHP, playerClass, EnnemiHP);
                        intDivinDuJoueur = player.Generer_Attaque_Player(difficulty, playerClass);
                        if (EnnemiAction == 2)
                        {
                            intDivinDuJoueur = 0;
                            intDivinDuMobs = mob.GénérerBloque(difficulty, choixDeLennemi);
                            // Animation de bloque du joueur et de l'attaque du joueur
                            EnnemiHP = EnnemiHP - intDivinDuJoueur - intDivinDuMobs;
                        }
                        
                        break;
                    case 2: // Block
                        break;
                    case 3: // Spell
                        break;
                    case 4: // Item
                        break;

                }
        private void BattleParThomas(int difficulty, int playerHP)
        {
            


        }
        // Fin du tung tung tung sahur


    }
}