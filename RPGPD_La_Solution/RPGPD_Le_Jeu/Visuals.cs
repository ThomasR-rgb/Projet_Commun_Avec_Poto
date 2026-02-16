using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;

namespace RPGPD_Le_Jeu
{
    // ==========================================
    // CLASSES VISUELLES & HELPER
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
    // PARTIE VISUELLE (Gameform, animations, rendu, etc)
    // ==========================================
    public partial class GameForm : Form
    {
        // ÉTATS GLOBAUX PARTAGÉS
        public PlayerClass _currentClass;
        public int _playerLevel = 1;
        public int _potions = 1;
        
        // Variables pour garder la vie et la mana entre les combats
        public int _currentHP;
        public int _currentMP;

        // Variables liées à l'affichage
        public int _uiPlayerHP;     
        public int _uiPlayerMaxHP;
        public int _uiPlayerMana;
        public int _uiPlayerMaxMana;
        public int _uiEnemyHP;
        public int _uiEnemyMaxHP;
        public string _enemyName = "Enemy";

        // Variable lue par le script de Tom
        public volatile int _lastActionSelected = 0; 
        public volatile int _selectedSpellId = 1; 

        // Visuels internes
        private Image? _whiteMageSprite;
        private Image? _fighterSprite;
        private Image? _darkMageSprite;
        
        private GameState _currentState;
        private System.Windows.Forms.Timer _gameTimer = null!;
        private float _globalTime = 0;
        private Random _rng = new Random();
        public bool _inputEnabled = false;

        // Particules & Effets
        private List<Particle> _particles = new List<Particle>();
        private List<PopupText> _popups = new List<PopupText>();
        private Queue<PopupText> _popupQueue = new Queue<PopupText>();
        private int _shakeDuration = 0;
        private int _shakeMagnitude = 0;
        private int _flashDuration = 0;
        private float _playerAnimOffset = 0;
        private float _enemyAnimOffset = 0;
        public bool _enemyIsBlocking = false;

        // Contrôles UI
        private Label lblTitle = null!;
        private Button btnStart = null!;
        private Button btnQuit = null!;
        private Button btnCredits = null!;
        private GroupBox grpClassSelect = null!;
        private Panel pnlEncounterSelect = null!;
        private BufferedPanel pnlBattleScene = null!;
        private Panel pnlBottomUI = null!;
        private BufferedPanel pnlWinScreen = null!;
        private BufferedPanel pnlCreditsOverlay = null!;
        private GroupBox grpActions = null!;
        private GroupBox grpSpellSelect = null!; 
        
        private Button btnFighter = null!;
        private Button btnWhiteMage = null!;
        private Button btnDarkMage = null!;
        private Button btnPathLeft = null!;
        private Button btnPathRight = null!;
        
        // Action Buttons
        private Button btnAttack = null!;
        private Button btnBlock = null!;
        private Button btnSpell = null!;
        private Button btnItem = null!;
        
        // Spell Buttons
        private Button btnSpell1 = null!;
        private Button btnSpell2 = null!;
        private Button btnSpell3 = null!;
        private Button btnSpell4 = null!;
        private Button btnSpellBack = null!;

        private Button btnWinReturn = null!;
        private Button btnCreditsClose = null!;
        private Label lblPlayerStats = null!;
        private Label lblEnemyStats = null!;
        private Label lblWinMessage = null!;
        private Label lblCreditsText = null!;
        private RichTextBox txtGameLog = null!;

        // Logique de sélection de mob
        private int _leftMobId;
        private int _rightMobId;
        private string _leftEnemyType = "";
        private string _rightEnemyType = "";

        public GameForm()
        {
            SetupWindow();
            try
            {
                string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
                _whiteMageSprite = LoadImg(Path.Combine(exeFolder, "white_mage.png"));
                _fighterSprite = LoadImg(Path.Combine(exeFolder, "fighter.png"));
                _darkMageSprite = LoadImg(Path.Combine(exeFolder, "dark_mage.png"));
            }
            catch { }

            InitializeComponents();
            InitializeAnimation();
            ShowMainMenu();
        }

        private Image? LoadImg(string path) {
            if(File.Exists(path)) return Image.FromFile(path);
            return null;
        }

        private void SetupWindow()
        {
            this.Text = "RudacoPG - The Adventure";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
            this.KeyPreview = true;
            this.KeyDown += HandleGlobalInput;
            this.DoubleBuffered = true;
        }

        private void InitializeAnimation()
        {
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 16;
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            _globalTime += 0.1f;
            if (_shakeDuration > 0) _shakeDuration--;
            if (_flashDuration > 0) _flashDuration--;

            UpdateParticles();
            UpdatePopups();

            _playerAnimOffset = Lerp(_playerAnimOffset, 0, 0.1f);
            _enemyAnimOffset = Lerp(_enemyAnimOffset, 0, 0.1f);

            if (_currentState == GameState.Battle) pnlBattleScene.Invalidate();
            if (_currentState == GameState.Victory || _currentState == GameState.GameOver) pnlWinScreen.Invalidate();
            if (_currentState == GameState.MainMenu) AnimateMainMenuTitle();
        }

        public void UpdateBattleUI(int hp, int maxHp, int mp, int maxMp, int mobHp, int mobMaxHp)
        {
            _uiPlayerHP = hp;
            _uiPlayerMaxHP = maxHp;
            _uiPlayerMana = mp;
            _uiPlayerMaxMana = maxMp;
            _uiEnemyHP = mobHp;
            _uiEnemyMaxHP = mobMaxHp;

            lblPlayerStats.Text = $"YOU (Lvl {_playerLevel})\nHP: {_uiPlayerHP}/{_uiPlayerMaxHP}\nMP: {_uiPlayerMana}/{_uiPlayerMaxMana}\nPotions: {_potions}";
            lblEnemyStats.Text = $"{_enemyName}\nHP: {_uiEnemyHP}/{_uiEnemyMaxHP}";
        }

      private void InputBtnClick(int actionId)
      {
        if (!_inputEnabled || _currentState != GameState.Battle) return;

        if (actionId == 3)
        {
             grpActions.Visible = false;
             grpSpellSelect.Visible = true;
             UpdateSpellButtons(); 
             return;
        }

        _lastActionSelected = actionId; 
    
        _inputEnabled = false; 
        ToggleActionButtons(false); 
      }

      private void SpellBtnClick(int spellId)
      {
          _selectedSpellId = spellId;
          grpSpellSelect.Visible = false;
          grpActions.Visible = true;
          
          _lastActionSelected = 3;
          _inputEnabled = false;
          ToggleActionButtons(false);
      }

      private void BackToActions()
      {
          grpSpellSelect.Visible = false;
          grpActions.Visible = true;
      }

      // Griser les spells inconnus
      private void UpdateSpellButtons()
      {
            btnSpell1.Text = "-"; btnSpell2.Text = "-"; btnSpell3.Text = "-"; btnSpell4.Text = "-";
            
            if (_currentClass == PlayerClass.Fighter) {
                btnSpell1.Text = "Magic Sword (1 MP)";
                btnSpell2.Text = "Cure (1 MP)";
            }
            else if (_currentClass == PlayerClass.WhiteMage) {
                btnSpell1.Text = "Fire Bolt (1 MP)";
                btnSpell2.Text = "Heal (1 MP)";
                btnSpell3.Text = "Divine Grace (2 MP)";
                btnSpell4.Text = "Smite (3 MP)";
            }
            else if (_currentClass == PlayerClass.DarkMage) {
                btnSpell1.Text = "Vampire Touch (1 MP)";
                btnSpell2.Text = "Reflect (2 MP)";
                btnSpell3.Text = "Ice Beam (2 MP)";
                btnSpell4.Text = "Kill (7 MP)";
            }

            // Loop pour désactiver les boutons "-"
            Button[] spells = { btnSpell1, btnSpell2, btnSpell3, btnSpell4 };
            foreach(var btn in spells)
            {
                if(btn.Text == "-") {
                    btn.Enabled = false;
                    btn.BackColor = Color.FromArgb(60, 60, 60); // Gris foncé
                    btn.ForeColor = Color.Gray;
                } else {
                    btn.Enabled = true;
                    btn.BackColor = Color.Indigo;
                    btn.ForeColor = Color.White;
                }
            }
      }

        public void Log(string message)
        {
            txtGameLog.AppendText($"> {message}\n");
            txtGameLog.ScrollToCaret();
        }

        public void TriggerAttackAnim(bool isPlayer)
        {
            if (isPlayer) _playerAnimOffset = 200;
            else _enemyAnimOffset = -200;
        }

        public void TriggerEffect(string type, int value = 0)
        {
            if (type == "HIT") {
                ShakeScreen(5, 5);
                SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height / 2, Color.Red, 20);
            }
            if (type == "CRIT") {
                ShakeScreen(15, 10);
                SpawnPopup("CRITICAL!", Color.Yellow, 36);
            }
            if (type == "BLOCK") {
                SpawnPopup("BLOCK", Color.Cyan);
            }
            if (type == "HEAL") {
                SpawnPopup($"+{value} HP", Color.LightGreen);
                SpawnParticles(200, pnlBattleScene.Height / 2, Color.LightGreen, 20);
            }
            if (type == "VICTORY") {
                SpawnPopup("VICTORY!", Color.Gold, 48);
                SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height / 2, Color.Gold, 50);
            }
        }

        public void ToggleActionButtons(bool enabled)
        {
            Color btnColor = enabled ? Color.White : Color.Gray;
            
            btnAttack.Enabled = enabled;
            btnBlock.Enabled = enabled;
            btnSpell.Enabled = enabled;
            btnItem.Enabled = enabled;

            btnAttack.ForeColor = btnColor;
            btnBlock.ForeColor = btnColor;
            btnSpell.ForeColor = btnColor;
            btnItem.ForeColor = btnColor;
        }

        private void StartGame(PlayerClass selectedClass)
        {
            _currentClass = selectedClass;
            _playerLevel = 1;
            _potions = 1; 

            // Initialiser les HP/MP ici pour le début du jeu
            Player p = new Player((int)selectedClass + 1);
            _currentHP = p.MaxHPPlayerScale(1, (int)selectedClass + 1);
            _currentMP = p.BaseMPPlayer(1, (int)selectedClass + 1);

            Log("You chose " + _currentClass.ToString() + "!");
            ShowEncounterSelection();
        }

        private void SelectPath(bool isLeft)
        {
            int mobId = isLeft ? _leftMobId : _rightMobId;
            _enemyName = isLeft ? _leftEnemyType : _rightEnemyType;
            StartBattleLogic(mobId);
        }

        private async void StartBattleLogic(int mobId)
        {
            _currentState = GameState.Battle;
            ShowBattleUI();
            
            int classId = (int)_currentClass + 1; 
            
            await BattleParThomas(_playerLevel, classId, mobId, _potions);

            _battlesWon++;
            
            // Regen seulement au Level Up
            if(_battlesWon % 3 == 0) {
                 _playerLevel++;
                 _potions = 1; 
                 
                 // On restore la vie et la mana au max
                 Player p = new Player(classId);
                 _currentHP = p.MaxHPPlayerScale(_playerLevel > 3 ? 3 : _playerLevel, classId);
                 _currentMP = p.BaseMPPlayer(_playerLevel > 3 ? 3 : _playerLevel, classId);

                 Log("LEVEL UP! Health, Mana & Potion restored.");
            }
            
            await Task.Delay(2000);
            ShowEncounterSelection();
        }

        private void BattleScene_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pnlBattleScene.Width;
            int h = pnlBattleScene.Height;

            DrawBackground(g, w, h);
            ApplyShake(g);

            DrawPlayer(g, w, h);
            DrawMob(g, w, h, _enemyName);

            DrawParticles(g);
            DrawPopups(g);
            DrawFlash(g, w, h);
        }

        private void DrawBackground(Graphics g, int w, int h)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(new Point(0, 0), new Point(w, h), Color.FromArgb(20, 0, 20), Color.Black))
            {
                g.FillRectangle(brush, 0, 0, w, h);
            }
        }

        private void ApplyShake(Graphics g)
        {
            if (_shakeDuration > 0)
            {
                int shakeX = _rng.Next(-_shakeMagnitude, _shakeMagnitude);
                int shakeY = _rng.Next(-_shakeMagnitude, _shakeMagnitude);
                g.TranslateTransform(shakeX, shakeY);
            }
        }

        private void DrawPlayer(Graphics g, int w, int h)
        {
            float x = 200 + _playerAnimOffset;
            float y = h / 2;

            Image? spriteToUse = null;
            if (_currentClass == PlayerClass.WhiteMage) spriteToUse = _whiteMageSprite;
            else if (_currentClass == PlayerClass.Fighter) spriteToUse = _fighterSprite;
            else if (_currentClass == PlayerClass.DarkMage) spriteToUse = _darkMageSprite;

            if (spriteToUse != null)
            {
                var oldMode = g.InterpolationMode;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(spriteToUse, x, y - 50, 100, 100);
                g.InterpolationMode = oldMode;
                return;
            }

            using (SolidBrush b = new SolidBrush(Color.SteelBlue)) g.FillEllipse(b, x, y, 80, 80);
            if (_currentClass == PlayerClass.Fighter)
                using (Pen p = new Pen(Color.Silver, 5)) g.DrawLine(p, x + 60, y + 40, x + 100, y + 10);
        }

        private void DrawMob(Graphics g, int w, int h, string name)
        {
            float breathe = (float)Math.Sin(_globalTime * 0.15f) * 5;
            float x = w - 300 + _enemyAnimOffset;
            float y = h / 2 - 50 + breathe;

            Brush mobColor = name.Contains("Gobelin") ? Brushes.ForestGreen : 
                             name.Contains("Squelette") ? Brushes.LightGray :
                             name.Contains("Boss") ? Brushes.DarkRed : Brushes.Gray;
            
            g.FillEllipse(mobColor, x, y, 100, 100);
            
            if (_enemyIsBlocking)
                using (Pen p = new Pen(Color.Cyan, 5)) g.DrawEllipse(p, x - 10, y - 10, 120, 120);
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
                p.Position.Y -= 1.0f;
                p.Life--;
                p.Alpha = Math.Min(1.0f, Math.Max(0.0f, p.Life / 60.0f));
                if (p.Scale < 1.0f) p.Scale += 0.1f;
                if (p.Life <= 0) _popups.RemoveAt(i);
            }
            if (_popups.Count == 0 && _popupQueue.Count > 0) _popups.Add(_popupQueue.Dequeue());
        }

        private void ShakeScreen(int magnitude, int duration) { _shakeMagnitude = magnitude; _shakeDuration = duration; }
        private void SpawnParticles(float x, float y, Color c, int count)
        {
            for (int i = 0; i < count; i++)
                _particles.Add(new Particle() { Position = new PointF(x, y), Velocity = new PointF(_rng.Next(-10, 11), _rng.Next(-10, 11)), Color = c, Size = _rng.Next(5, 15), Life = 30 });
        }
        private void SpawnPopup(string text, Color c, int size = 24)
        {
            _popupQueue.Enqueue(new PopupText() { Text = text, Position = new PointF(pnlBattleScene.Width / 2, pnlBattleScene.Height / 2), Color = c, Scale = 0.1f, Life = 80, Alpha = 1.0f });
        }
        private void DrawParticles(Graphics g) {
            foreach (var p in _particles) {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(Math.Min(255, Math.Max(0, (int)(p.Life * 8.5))), p.Color)))
                    g.FillEllipse(b, p.Position.X, p.Position.Y, p.Size, p.Size);
            }
        }
        private void DrawPopups(Graphics g) {
            foreach (var p in _popups) {
                using (Font font = new Font("Impact", 60 * p.Scale, FontStyle.Bold)) using (Brush b = new SolidBrush(Color.FromArgb((int)(p.Alpha * 255), p.Color))) {
                    SizeF size = g.MeasureString(p.Text, font);
                    g.DrawString(p.Text, font, b, p.Position.X - size.Width / 2, p.Position.Y - size.Height / 2);
                }
            }
        }
        private void DrawFlash(Graphics g, int w, int h) { if (_flashDuration > 0) using (SolidBrush b = new SolidBrush(Color.FromArgb(100, 255, 255, 255))) g.FillRectangle(b, 0, 0, w, h); }
        private float Lerp(float a, float b, float f) { return a * (1 - f) + b * f; }

        private void AnimateMainMenuTitle() {
            float scale = 1.0f + (float)Math.Sin(_globalTime * 2) * 0.05f;
            lblTitle.Font = new Font(lblTitle.Font.FontFamily, 72 * scale, FontStyle.Italic);
        }

        // ==========================================
        // UI SETUP
        // ==========================================
        private void ShowMainMenu() { _currentState = GameState.MainMenu; lblTitle.Visible = true; btnStart.Visible = true; btnQuit.Visible = true; btnCredits.Visible = true; grpClassSelect.Visible = false; pnlEncounterSelect.Visible = false; pnlWinScreen.Visible = false; pnlCreditsOverlay.Visible = false; HideBattleUI(); }
        private void ShowClassSelection() { _currentState = GameState.ClassSelection; btnStart.Visible = false; btnQuit.Visible = false; btnCredits.Visible = false; grpClassSelect.Visible = true; lblTitle.Visible = false; }
        private void ShowEncounterSelection() {
            _currentState = GameState.EncounterSelect; grpClassSelect.Visible = false; HideBattleUI(); pnlEncounterSelect.Visible = true; pnlWinScreen.Visible = false;
            int currentDiff = _playerLevel > 3 ? 3 : _playerLevel;
            Mobs mobGenerator = new Mobs(currentDiff, 1, 1);
            _leftMobId = mobGenerator.Générer_choix_Mob();
            _rightMobId = mobGenerator.Générer_choix_Mob();
            _leftEnemyType = GetMobName(currentDiff, _leftMobId);
            _rightEnemyType = GetMobName(currentDiff, _rightMobId);
            btnPathLeft.Text = $"PATH A\n\n{_leftEnemyType}"; btnPathRight.Text = $"PATH B\n\n{_rightEnemyType}";
        }
        private void ShowBattleUI() { lblTitle.Visible = false; grpClassSelect.Visible = false; pnlEncounterSelect.Visible = false; pnlWinScreen.Visible = false; pnlBattleScene.Visible = true; pnlBottomUI.Visible = true; }
        private void HideBattleUI() { pnlBattleScene.Visible = false; pnlBottomUI.Visible = false; }
        private void WinScreen_Paint(object? sender, PaintEventArgs e) { /* ... */ }

        private void InitializeComponents() {
            
            int w = Screen.PrimaryScreen?.Bounds.Width ?? 1920; int h = Screen.PrimaryScreen?.Bounds.Height ?? 1080; int cx = w / 2; int cy = h / 2;
            lblTitle = new Label() { Text = "RudacoPG", Font = new Font("Impact", 72, FontStyle.Italic), Size = new Size(w, 200), TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.Cyan, Location = new Point(0, cy - 250) }; this.Controls.Add(lblTitle);
            
            btnStart = CreateMenuButton("PLAY", cx - 150, cy, (s, e) => ShowClassSelection());
            btnQuit = CreateMenuButton("QUIT", cx - 150, cy + 160, (s, e) => Application.Exit());
            btnCredits = CreateMenuButton("CREDITS", cx - 150, cy + 80, (s, e) => { pnlCreditsOverlay.Visible = true; pnlCreditsOverlay.BringToFront(); });

            pnlCreditsOverlay = new BufferedPanel() { Size = new Size(600, 400), Location = new Point(cx - 300, cy - 200), BackColor = Color.FromArgb(240, 20, 20, 20), Visible = false }; 
            this.Controls.Add(pnlCreditsOverlay);
            
            // PATCH: Credits ajustés
            lblCreditsText = new Label() {
            Text = "Created by Thomas & Félix\n\nArt: Alana\nCode: C#\n\nThanks for playing!",
            Font = new Font("Arial", 16, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
           };
            pnlCreditsOverlay.Controls.Add(lblCreditsText);
            
            // PATCH: Bouton Close remonté
            btnCreditsClose = CreateMenuButton("CLOSE", 150, 310, (s, e) => pnlCreditsOverlay.Visible = false); 
            pnlCreditsOverlay.Controls.Add(btnCreditsClose);


            grpClassSelect = new GroupBox() { Text = "CHOOSE YOUR CLASS", Font = new Font("Arial", 16, FontStyle.Bold), ForeColor = Color.White, Size = new Size(600, 400), Location = new Point(cx - 300, cy - 200), Visible = false, BackColor = Color.FromArgb(20, 20, 20) };
            btnFighter = CreateClassButton("FIGHTER", 80, Color.DarkRed); btnFighter.Click += (s, e) => StartGame(PlayerClass.Fighter);
            btnWhiteMage = CreateClassButton("WHITE MAGE", 160, Color.LightBlue); btnWhiteMage.Click += (s, e) => StartGame(PlayerClass.WhiteMage);
            btnDarkMage = CreateClassButton("DARK MAGE", 240, Color.Purple); btnDarkMage.Click += (s, e) => StartGame(PlayerClass.DarkMage);
            grpClassSelect.Controls.AddRange(new Control[] { btnFighter, btnWhiteMage, btnDarkMage }); this.Controls.Add(grpClassSelect);

            pnlEncounterSelect = new Panel() { Dock = DockStyle.Fill, BackColor = Color.Black, Visible = false }; this.Controls.Add(pnlEncounterSelect);
            btnPathLeft = CreatePathButton(0, 0, w / 2, h); btnPathLeft.Click += (s, e) => SelectPath(true); pnlEncounterSelect.Controls.Add(btnPathLeft);
            btnPathRight = CreatePathButton(w / 2, 0, w / 2, h); btnPathRight.Click += (s, e) => SelectPath(false); pnlEncounterSelect.Controls.Add(btnPathRight);

            pnlBottomUI = new Panel() { Dock = DockStyle.Bottom, Height = 250, BackColor = Color.Transparent }; this.Controls.Add(pnlBottomUI);
            txtGameLog = new RichTextBox() { Location = new Point(20, 20), Size = new Size(w / 2 - 40, 210), ReadOnly = true, BackColor = Color.FromArgb(10, 10, 10), ForeColor = Color.LightGray, Font = new Font("Consolas", 12), BorderStyle = BorderStyle.None }; pnlBottomUI.Controls.Add(txtGameLog);
            
            // ACTION GROUP
            grpActions = new GroupBox() { Location = new Point(w / 2, 10), Size = new Size(w / 2 - 20, 230), BackColor = Color.Transparent }; pnlBottomUI.Controls.Add(grpActions);
            
            int bx = 20, by = 20, bw = (grpActions.Width / 2) - 40, bh = 80;
            btnAttack = CreateActionButton("ATTACK", bx, by, bw, bh, Color.DarkRed); btnAttack.Click += (s, e) => InputBtnClick(1);
            btnBlock = CreateActionButton("BLOCK", bx + bw + 20, by, bw, bh, Color.DarkBlue); btnBlock.Click += (s, e) => InputBtnClick(2);
            btnSpell = CreateActionButton("SPELL", bx, by + bh + 20, bw, bh, Color.Purple); btnSpell.Click += (s, e) => InputBtnClick(3);
            btnItem = CreateActionButton("ITEM", bx + bw + 20, by + bh + 20, bw, bh, Color.DarkGreen); btnItem.Click += (s, e) => InputBtnClick(4);
            grpActions.Controls.AddRange(new Control[] { btnAttack, btnBlock, btnSpell, btnItem });

            // PATCH: Mini menu de spells
            grpSpellSelect = new GroupBox() { Location = new Point(w / 2, 10), Size = new Size(w / 2 - 20, 230), BackColor = Color.Transparent, Visible = false }; pnlBottomUI.Controls.Add(grpSpellSelect);
            btnSpell1 = CreateActionButton("-", bx, by, bw, bh, Color.Indigo); btnSpell1.Click += (s, e) => SpellBtnClick(1);
            btnSpell2 = CreateActionButton("-", bx + bw + 20, by, bw, bh, Color.Indigo); btnSpell2.Click += (s, e) => SpellBtnClick(2);
            btnSpell3 = CreateActionButton("-", bx, by + bh + 20, bw, bh, Color.Indigo); btnSpell3.Click += (s, e) => SpellBtnClick(3);
            btnSpell4 = CreateActionButton("-", bx + bw + 20, by + bh + 20, bw, bh, Color.Indigo); btnSpell4.Click += (s, e) => SpellBtnClick(4);
            
            btnSpellBack = CreateMenuButton("BACK", 0, 0, (s, e) => BackToActions());
            btnSpellBack.Size = new Size(100, 30); btnSpellBack.Location = new Point(grpSpellSelect.Width - 110, 0); btnSpellBack.Font = new Font("Arial", 10);
            
            grpSpellSelect.Controls.AddRange(new Control[] { btnSpell1, btnSpell2, btnSpell3, btnSpell4, btnSpellBack });


            pnlBattleScene = new BufferedPanel() { Dock = DockStyle.Fill, BackColor = Color.Black, Visible = false }; pnlBattleScene.Paint += BattleScene_Paint; this.Controls.Add(pnlBattleScene);
            lblEnemyStats = new Label { AutoSize = true, Location = new Point(w - 300, 50), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Red, BackColor = Color.Transparent }; pnlBattleScene.Controls.Add(lblEnemyStats);
            lblPlayerStats = new Label { AutoSize = true, Location = new Point(50, 50), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Cyan, BackColor = Color.Transparent }; pnlBattleScene.Controls.Add(lblPlayerStats);

            pnlWinScreen = new BufferedPanel() { Dock = DockStyle.Fill, BackColor = Color.Black, Visible = false }; pnlWinScreen.Paint += WinScreen_Paint; this.Controls.Add(pnlWinScreen); pnlWinScreen.BringToFront();
            lblWinMessage = new Label() { Text = "VICTORY!", Font = new Font("Impact", 72, FontStyle.Italic), ForeColor = Color.Gold, Size = new Size(w, 300), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, cy - 200), BackColor = Color.Transparent }; pnlWinScreen.Controls.Add(lblWinMessage);
            btnWinReturn = CreateMenuButton("RETURN", cx - 150, cy + 150, (s, e) => ShowMainMenu()); pnlWinScreen.Controls.Add(btnWinReturn);
        }

        private Button CreateMenuButton(string text, int x, int y, EventHandler onClick) { Button btn = new Button() { Text = text, Location = new Point(x, y), Size = new Size(300, 70), Font = new Font("Arial", 18, FontStyle.Bold), BackColor = Color.FromArgb(40, 40, 40), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; btn.Click += onClick; this.Controls.Add(btn); return btn; }
        private Button CreateClassButton(string text, int y, Color c) { return new Button() { Text = text, Location = new Point(100, y), Size = new Size(400, 70), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 14, FontStyle.Bold) }; }
        private Button CreateActionButton(string text, int x, int y, int w, int h, Color c) { return new Button() { Text = text, Location = new Point(x, y), Size = new Size(w, h), Font = new Font("Arial", 16, FontStyle.Bold), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; }
        private Button CreatePathButton(int x, int y, int w, int h) { return new Button() { Location = new Point(x, y), Size = new Size(w, h), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.White, Font = new Font("Courier New", 24, FontStyle.Bold) }; }
        private void HandleGlobalInput(object? sender, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) { ShowMainMenu(); } }
        
        private string GetMobName(int d, int c) { 
            if(d == 1) {
                if (c == 1) return "Gobelin"; 
                if (c == 2) return "Squelette";
                if (c == 3) return "Gros Rat";
                if (c == 4) return "Gobelin Swarm";
                if (c == 5) return "Porte Méchante";
            }
            if(d == 2) {
                if (c == 1) return "Orc";
                if (c == 2) return "Slime";
                if (c == 3) return "Mage Gobelin";
                if (c == 4) return "Géant";
                if (c == 5) return "Gargouille";
            }
            if(d == 3) {
                if (c == 1) return "Troll";
                if (c == 2) return "Champion Squelette";
                if (c == 3) return "Sirène";
                if (c == 4) return "Démon";
                if (c == 5) return "Dark Sorcerer";
            }
            return "Monster"; 
        } 
    }
}