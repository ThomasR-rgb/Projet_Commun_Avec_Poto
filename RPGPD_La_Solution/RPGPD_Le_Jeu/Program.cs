using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq; 

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
        Victory, // État de victoire
        GameOver,
        Credits // Crédit pour l'overlay
    }

    // Classes pour les juicy animations
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
        public string Text;
        public PointF Position;
        public Color Color;
        public float Scale;
        public int Life;
        public float Alpha;
    }

    // Je fais ici des panels spéciaux pour éviter le crash total de l'armageddon
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
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

        // Ennemi actuel (vieille version)
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

        // Variables d'animations
        private System.Windows.Forms.Timer _gameTimer = null!;
        private float _globalTime = 0;
        private List<Particle> _particles = new List<Particle>();
        private List<PopupText> _popups = new List<PopupText>(); 
        private Queue<PopupText> _popupQueue = new Queue<PopupText>(); // La queue des popups
        private Random _rng = new Random();
        
        // Shaky Shakouille
        private int _shakeDuration = 0;
        private int _shakeMagnitude = 0;

        // État des sprites
        private float _playerAnimOffset = 0;
        private float _enemyAnimOffset = 0;
        private Color _flashColor = Color.Transparent;
        private int _flashDuration = 0;

        // TOUS LES UI
        private Label lblTitle = null!;
        private Button btnStart = null!;
        private Button btnQuit = null!;
        private Button btnCredits = null!; 
        
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
        private BufferedPanel pnlBattleScene = null!; // Pour le dessin custom
        private Label lblPlayerStats = null!;
        private Label lblEnemyStats = null!;
        
        private GroupBox grpActions = null!;   
        private Button btnAttack = null!;
        private Button btnBlock = null!;
        private Button btnSpell = null!;
        private Button btnItem = null!;
        
        // UI de Victoire
        private BufferedPanel pnlWinScreen = null!;
        private Label lblWinMessage = null!;
        private Button btnWinReturn = null!;

        // UI de Credits (Maintenant en Popup triple chocolat)
        private BufferedPanel pnlCreditsOverlay = null!;
        private Label lblCreditsText = null!;
        private Button btnCreditsClose = null!;

        private RichTextBox txtGameLog = null!;

        public GameForm()
        {
            // Configuration de la fenêtre - EN FULLSCREEN DÉSORMAIS
            this.Text = "RudacoPG - The Adventure";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            
            // Le fond il est TOUT NOIR
            this.BackColor = Color.Black; 
            
            // Tu peux maintenant t'échapper du jeu avec Échap
            this.KeyPreview = true;
            this.KeyDown += GameForm_KeyDown;

            // Pour éviter le flickering
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponents();
            InitializeAnimation(); 
            ShowMainMenu();
        }

        private void InitializeAnimation()
        {
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 16; // ~60 FPS (source: tkt frer)
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        // On check pour la touche escape
        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // On reset le chaos
                _particles.Clear();
                _popups.Clear();
                _popupQueue.Clear();
                _shakeDuration = 0;
                
                // De retour chez nous
                ShowMainMenu();
            }
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            _globalTime += 0.1f;

            // Logique du shaky shakouille
            if (_shakeDuration > 0) _shakeDuration--;
            if (_flashDuration > 0) _flashDuration--;

            // Particules incroyables
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                Particle p = _particles[i];
                p.Position.X += p.Velocity.X;
                p.Position.Y += p.Velocity.Y;
                p.Life--;
                p.Size *= 0.95f; 
                if (p.Life <= 0) _particles.RemoveAt(i);
            }

            // On update les popups
            for (int i = _popups.Count - 1; i >= 0; i--)
            {
                PopupText p = _popups[i];
                p.Position.Y -= 1.0f; // Ça float un peu comme mon service au Volley (faux)
                p.Life--;
                
                // On s'assure que l'Alpha ne dépasse jamais 1.0f, la dernière fois ça a fait crash mon ordi
                float rawAlpha = p.Life / 60.0f;
                p.Alpha = Math.Min(1.0f, Math.Max(0.0f, rawAlpha)); 

                if (p.Scale < 1.0f) p.Scale += 0.1f; // Pop in d'animation
                if (p.Life <= 0) _popups.RemoveAt(i);
            }

            // On utilise la queue pour les popups
            if (_popups.Count == 0 && _popupQueue.Count > 0)
            {
                _popups.Add(_popupQueue.Dequeue());
            }

            // Animation des ennemis et joueurs
            _playerAnimOffset = Lerp(_playerAnimOffset, 0, 0.1f);
            _enemyAnimOffset = Lerp(_enemyAnimOffset, 0, 0.1f);

            // On redessine selon l'état du jeu
            if (_currentState == GameState.Battle) pnlBattleScene.Invalidate();
            
            // On veut un Game Over animé
            if (_currentState == GameState.Victory || _currentState == GameState.GameOver) pnlWinScreen.Invalidate();
            
            // On anime le titre dans le menu de base
            if (_currentState == GameState.MainMenu)
            {
                float scale = 1.0f + (float)Math.Sin(_globalTime * 2) * 0.05f;
                lblTitle.Font = new Font(lblTitle.Font.FontFamily, 72 * scale, FontStyle.Italic);
                // Double recentrageage
                CenterTitle();
            }
        }

        private float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        // On provoque le shaky shakouille
        private void ShakeScreen(int magnitude, int duration)
        {
            _shakeMagnitude = magnitude;
            _shakeDuration = duration;
        }

        // MACRON EXPLOSION
        private void SpawnParticles(int x, int y, Color c, int count)
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

        // On affiche le popup de texte
        private void SpawnPopup(string text, Color c, int size = 24)
        {
            // J'ai enlevé la position random pour éviter que ça soit dégueulasse
            _popupQueue.Enqueue(new PopupText()
            {
                Text = text,
                Position = new PointF(pnlBattleScene.Width / 2, pnlBattleScene.Height / 2),
                Color = c,
                Scale = 0.1f,
                Life = 80, // Ti peu plus long
                Alpha = 1.0f
            });
        }

        // Initialisation de tous les widgets
        private void InitializeComponents()
        {
            Rectangle screenBounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            int w = screenBounds.Width;
            int h = screenBounds.Height;
            int cx = w / 2;
            int cy = h / 2;

            // 1. Titre et Menu Principal
            lblTitle = new Label();
            lblTitle.Text = "RudacoPG";
            lblTitle.Font = new Font("Impact", 72, FontStyle.Italic); 
            lblTitle.AutoSize = false; // On check la taille manuellement
            lblTitle.Size = new Size(w, 200); // Largeur max
            lblTitle.TextAlign = ContentAlignment.MiddleCenter; // Maintenant je centre automatiquement
            lblTitle.ForeColor = Color.Cyan;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Location = new Point(0, cy - 250);
            this.Controls.Add(lblTitle);

            btnStart = CreateButton("PLAY", cx - 150, cy, (s, e) => ShowClassSelection());
            btnCredits = CreateButton("CREDITS", cx - 150, cy + 80, (s, e) => ShowCredits());
            btnQuit = CreateButton("QUIT", cx - 150, cy + 160, (s, e) => Application.Exit());
            
            // Les crédits en overlay (moins moche)
            pnlCreditsOverlay = new BufferedPanel();
            pnlCreditsOverlay.Size = new Size(600, 400);
            pnlCreditsOverlay.Location = new Point(cx - 300, cy - 200);
            pnlCreditsOverlay.BackColor = Color.FromArgb(240, 20, 20, 20); // Semi-transparent
            pnlCreditsOverlay.Visible = false;
            pnlCreditsOverlay.Paint += (s, e) => {
                e.Graphics.DrawRectangle(Pens.Cyan, 0,0, 599, 399);
            };
            this.Controls.Add(pnlCreditsOverlay);

            lblCreditsText = new Label();
            lblCreditsText.Text = "Made with LOVE by\n\nFélix Lehoux\n&\nThomas Rudacovitch\n\nFor the Ultimate RPG Experience. (trust)";
            lblCreditsText.Font = new Font("Arial", 18, FontStyle.Bold);
            lblCreditsText.ForeColor = Color.White;
            lblCreditsText.AutoSize = false;
            lblCreditsText.Size = new Size(600, 300);
            lblCreditsText.TextAlign = ContentAlignment.MiddleCenter;
            lblCreditsText.Location = new Point(0, 20);
            pnlCreditsOverlay.Controls.Add(lblCreditsText);

            btnCreditsClose = CreateButton("CLOSE", 150, 320, (s, e) => HideCredits());
            pnlCreditsOverlay.Controls.Add(btnCreditsClose);

            // 2. Sélection de Classe
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

            // Split le screen
            pnlEncounterSelect = new Panel();
            pnlEncounterSelect.Dock = DockStyle.Fill; 
            pnlEncounterSelect.BackColor = Color.Black;
            pnlEncounterSelect.Visible = false;
            this.Controls.Add(pnlEncounterSelect);

            // Bouton Gauche (Screen splité)
            btnPathLeft = new Button();
            btnPathLeft.Location = new Point(0, 0);
            btnPathLeft.Size = new Size(w/2, h); 
            btnPathLeft.FlatStyle = FlatStyle.Flat;
            btnPathLeft.FlatAppearance.BorderSize = 0;
            btnPathLeft.BackColor = Color.FromArgb(20, 20, 20);
            btnPathLeft.ForeColor = Color.White;
            btnPathLeft.Font = new Font("Courier New", 24, FontStyle.Bold);
            btnPathLeft.Click += (s, e) => SelectPath(true);
            btnPathLeft.MouseEnter += (s,e) => btnPathLeft.BackColor = Color.FromArgb(40,40,40);
            btnPathLeft.MouseLeave += (s,e) => btnPathLeft.BackColor = Color.FromArgb(20,20,20);
            pnlEncounterSelect.Controls.Add(btnPathLeft);

            // Bouton Droit (Screen splité)
            btnPathRight = new Button();
            btnPathRight.Location = new Point(w/2, 0);
            btnPathRight.Size = new Size(w/2, h); 
            btnPathRight.FlatStyle = FlatStyle.Flat;
            btnPathRight.FlatAppearance.BorderSize = 0;
            btnPathRight.BackColor = Color.FromArgb(15, 15, 15);
            btnPathRight.ForeColor = Color.White;
            btnPathRight.Font = new Font("Courier New", 24, FontStyle.Bold);
            btnPathRight.Click += (s, e) => SelectPath(false);
            btnPathRight.MouseEnter += (s,e) => btnPathRight.BackColor = Color.FromArgb(35,35,35);
            btnPathRight.MouseLeave += (s,e) => btnPathRight.BackColor = Color.FromArgb(15,15,15);
            pnlEncounterSelect.Controls.Add(btnPathRight);

            // 3. Phase de pétage de yeules
            pnlBattleScene = new BufferedPanel();
            pnlBattleScene.Dock = DockStyle.Top;
            pnlBattleScene.Height = h - 250; 
            pnlBattleScene.BackColor = Color.Black;
            pnlBattleScene.Visible = false;
            pnlBattleScene.Paint += BattleScene_Paint; 
            this.Controls.Add(pnlBattleScene);

            // Stats Affichées
            lblEnemyStats = new Label { Text = "", AutoSize = true, Location = new Point(w - 300, 50), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Red, BackColor = Color.Transparent };
            pnlBattleScene.Controls.Add(lblEnemyStats);

            lblPlayerStats = new Label { Text = "", AutoSize = true, Location = new Point(50, 50), Font = new Font("Consolas", 18, FontStyle.Bold), ForeColor = Color.Cyan, BackColor = Color.Transparent };
            pnlBattleScene.Controls.Add(lblPlayerStats);

            // 4. Zone de Log
            txtGameLog = new RichTextBox();
            txtGameLog.Location = new Point(20, h - 220);
            txtGameLog.Size = new Size(w/2 - 40, 200);
            txtGameLog.ReadOnly = true;
            txtGameLog.BackColor = Color.FromArgb(10, 10, 10);
            txtGameLog.ForeColor = Color.LightGray;
            txtGameLog.Font = new Font("Consolas", 12);
            txtGameLog.BorderStyle = BorderStyle.None;
            txtGameLog.Visible = false;
            this.Controls.Add(txtGameLog);

            // 5. Menu d'action
            grpActions = new GroupBox();
            grpActions.Text = "";
            grpActions.Location = new Point(w/2, h - 230);
            grpActions.Size = new Size(w/2 - 20, 210);
            grpActions.Visible = false;
            grpActions.BackColor = Color.Transparent;
            this.Controls.Add(grpActions);

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

            // Écran de victoire (Ma plus belle création à ce jour)
            pnlWinScreen = new BufferedPanel();
            pnlWinScreen.Dock = DockStyle.Fill;
            pnlWinScreen.BackColor = Color.Black;
            pnlWinScreen.Visible = false;
            pnlWinScreen.Paint += WinScreen_Paint;
            this.Controls.Add(pnlWinScreen);

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

            btnWinReturn = CreateButton("RETURN TO MAIN MENU", cx - 150, cy + 150, (s, e) => ShowMainMenu());
            pnlWinScreen.Controls.Add(btnWinReturn);
        }

        private void CenterTitle()
        {
            // Cette fonction-là me fais juste rire tellement elle est vide mais elle sert actually à quelque chose plus haut
        }


        // LES ANIMATIONS DE PRO DE FÉLIX

        private void BattleScene_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = pnlBattleScene.Width;
            int h = pnlBattleScene.Height;

            // 1. Je dessine le fond en mode spatial
            int offset = (int)(_globalTime * 2) % w;
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

            // Shaky Shakouille
            int shakeX = 0, shakeY = 0;
            if(_shakeDuration > 0)
            {
                shakeX = _rng.Next(-_shakeMagnitude, _shakeMagnitude);
                shakeY = _rng.Next(-_shakeMagnitude, _shakeMagnitude);
            }
            g.TranslateTransform(shakeX, shakeY);

            // 2. Je dessine le joueur 
            float breathe = (float)Math.Sin(_globalTime * 0.2f) * 5;
            float playerX = 200 + _playerAnimOffset;
            float playerY = h/2;
            DrawPlayer(g, playerX, playerY + breathe, _currentClass);

            // 3. Je dessine l'ennemi
            float enemyBreathe = (float)Math.Sin(_globalTime * 0.15f + 2) * 8;
            float enemyX = w - 300 + _enemyAnimOffset;
            float enemyY = h/2 - 50;
            DrawMob(g, enemyX, enemyY + enemyBreathe, _enemyName);

            // 4. Je dessine les particules
            foreach (var p in _particles)
            {
                // Ellipse de sécurité pour les particules
                int alpha = Math.Min(255, Math.Max(0, (int)(p.Life * 8.5)));
                using(SolidBrush b = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                {
                    g.FillEllipse(b, p.Position.X, p.Position.Y, p.Size, p.Size);
                }
            }

            // 5. Je dessine les popups (Level Up, Damage, etc.)
            foreach(var p in _popups)
            {
                // Distance de sécurité pour les popups
                int alpha = Math.Min(255, Math.Max(0, (int)(p.Alpha * 255)));
                
                // JE LES METS PLUS GROS ET VISIBLES
                using(Font font = new Font("Impact", 60 * p.Scale, FontStyle.Bold))
                using(Brush b = new SolidBrush(Color.FromArgb(alpha, p.Color)))
                {
                    // On centre le tout
                    SizeF size = g.MeasureString(p.Text, font);
                    g.DrawString(p.Text, font, b, p.Position.X - size.Width/2, p.Position.Y - size.Height/2);
                }
            }

            // 6. J'ajoute un effet de flash
            if(_flashDuration > 0)
            {
                using(SolidBrush b = new SolidBrush(Color.FromArgb(100, 255, 255, 255)))
                {
                    g.FillRectangle(b, 0, 0, w, h);
                }
            }
        }

        private void WinScreen_Paint(object? sender, PaintEventArgs e)
        {
            // Fond animé pendant la victoire
            Graphics g = e.Graphics;
            int w = pnlWinScreen.Width;
            int h = pnlWinScreen.Height;
            
            if (_currentState == GameState.Victory)
            {
                float hue = (_globalTime * 2) % 360;
                // Le souffle de Morphée en personne
                Random r = new Random(4321);
                for(int i=0; i<100; i++)
                {
                    int x = r.Next(w);
                    int y = (int)((r.Next(h) + _globalTime * 5) % h);
                    g.FillEllipse(Brushes.Gold, x, y, 5, 5);
                }
            }
            else if (_currentState == GameState.GameOver)
            {
                // Animation de défaite (WOMP WOMP)
                
                // Fond rouge menaçant qui pulse de façon menaçante
                int redPulse = 50 + (int)(Math.Sin(_globalTime) * 20);
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(redPulse, 0, 0)))
                {
                    g.FillRectangle(bg, 0, 0, w, h);
                }

                // Pluie de tristesse
                Random r = new Random(666); // (Méga important pour lancer une curse sur le joueur dans la vraie vie)
                for (int i = 0; i < 150; i++)
                {
                    int x = r.Next(w);
                    // La pluie tombe vite, très vite
                    int speed = r.Next(10, 25);
                    int y = (int)((r.Next(h) + _globalTime * speed) % h);
                    
                    // Couleur gris/rouge foncé
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(100, 150, 0, 0)))
                    {
                        g.FillRectangle(b, x, y, 2, 15);
                    }
                }

                // Titre
                string sadText = "Womp Womp...";
                using (Font f = new Font("Courier New", 36, FontStyle.Bold))
                {
                    SizeF size = g.MeasureString(sadText, f);
                    float txtX = (w - size.Width) / 2;
                    float txtY = (h / 2) + 80; // Juste en dessous du "GAME OVER"

                    // Shaky Shakouille de l'humilialtion
                    float shakeX = (float)Math.Sin(_globalTime * 10) * 2;
                    float shakeY = (float)Math.Cos(_globalTime * 15) * 2;

                    g.DrawString(sadText, f, Brushes.Gray, txtX + shakeX, txtY + shakeY);
                }
            }
        }

        private void DrawPlayer(Graphics g, float x, float y, PlayerClass cls)
        {
            using(SolidBrush b = new SolidBrush(Color.SteelBlue))
                g.FillEllipse(b, x, y, 80, 80);
            
            using(SolidBrush b = new SolidBrush(Color.PeachPuff))
                g.FillEllipse(b, x+15, y-30, 50, 50);

            if (cls == PlayerClass.Fighter)
            {
                using(Pen p = new Pen(Color.Silver, 5))
                    g.DrawLine(p, x+60, y+40, x+100, y+10);
            }
            else if (cls == PlayerClass.DarkMage)
            {
                PointF[] hat = { new PointF(x+10, y-30), new PointF(x+70, y-30), new PointF(x+40, y-80) };
                g.FillPolygon(Brushes.Purple, hat);
            }
            else // Mage Blanc Suprématiste
            {
                using(Pen p = new Pen(Color.Gold, 3))
                    g.DrawEllipse(p, x+15, y-50, 50, 10);
            }

            if ((int)(_globalTime * 10) % 50 > 2)
            {
                g.FillEllipse(Brushes.Black, x+30, y-15, 5, 5);
                g.FillEllipse(Brushes.Black, x+50, y-15, 5, 5);
            }
        }

        private void DrawMob(Graphics g, float x, float y, string name)
        {
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

        private Button CreateButton(string text, int x, int y, EventHandler onClick)
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
            this.Controls.Add(btn);
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

        // GESTION DES ÉCRANS
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

        private void ShowWinScreen()
        {
            _currentState = GameState.Victory;
            HideBattleUI();
            pnlEncounterSelect.Visible = false;
            pnlWinScreen.Visible = true;
            pnlWinScreen.BringToFront(); // Je m'assure que c'est sur le top
        }

        private void ShowCredits()
        {
            pnlCreditsOverlay.Visible = true;
            pnlCreditsOverlay.BringToFront();
        }

        private void HideCredits()
        {
            pnlCreditsOverlay.Visible = false;
        }
        
        private void ShowEncounterSelection()
        {
            _currentState = GameState.EncounterSelect;
            grpClassSelect.Visible = false;
            HideBattleUI();
            pnlEncounterSelect.Visible = true;
            pnlWinScreen.Visible = false;
            
            Random rnd = new Random();

            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;

            Mobs mobGenerator = new Mobs(currentDiff);

            _leftMobId = mobGenerator.Générer_choix_Mob();
            _rightMobId = mobGenerator.Générer_choix_Mob();

            _leftEnemyType = GetMobName(currentDiff, _leftMobId);
            _rightEnemyType = GetMobName(currentDiff, _rightMobId);

            bool leftJammed = rnd.Next(0, 100) < 15;
            bool rightJammed = rnd.Next(0, 100) < 15;

            string leftText = leftJammed ? "?? JAMMED ??" : _leftEnemyType;
            string rightText = rightJammed ? "?? JAMMED ??" : _rightEnemyType;

            btnPathLeft.Text = $"PATH A\n\nStrange Corridor.\n\nSenses detect:\n{leftText}";
            btnPathRight.Text = $"PATH B\n\nDark Corridor...\n\nSenses detect:\n{rightText}";
            
            if (leftJammed) btnPathLeft.ForeColor = Color.Gray;
            else btnPathLeft.ForeColor = Color.White;

            if (rightJammed) btnPathRight.ForeColor = Color.Gray;
            else btnPathRight.ForeColor = Color.White;
        }

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
            pnlEncounterSelect.Visible = false; 
            pnlWinScreen.Visible = false;
            pnlBattleScene.Visible = true;
            txtGameLog.Visible = true;
            grpActions.Visible = true;
        }


        // LOGIQUE DU JEU

        private void StartGame(PlayerClass selectedClass)
        {
            _currentClass = selectedClass;
            _playerLevel = 1;
            _battlesWon = 0;
            _potions = 3;

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
            _enemyIsBlocking = false; 
            
            // Je reset les boutons pour pas que ça stall
            grpActions.Enabled = true;

            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;
            
            Mobs mobGenerator = new Mobs(currentDiff);
            
            _enemyMaxHP = mobGenerator.Générer_HP(currentDiff, _selectedMobId);
            _enemyDamage = mobGenerator.Générer_Attaque(currentDiff, _selectedMobId);
            _enemyHP = _enemyMaxHP;

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
        private async void PlayerTurn(string action)
        {
            if (_currentState != GameState.Battle) return;

            // JE DIS NON AU SPAM
            grpActions.Enabled = false; 

            if(action == "Attack") _playerAnimOffset = 200; 
            
            await Task.Delay(300); // Délais d'attente

            int damageDealt = 0;
            bool isBlocking = false;

            switch (action)
            {
                case "Attack":
                    Random rnd = new Random();
                    if (_currentClass == PlayerClass.Fighter)
                    {
                        damageDealt = rnd.Next(3, 7);
                    }
                    else
                    {
                        damageDealt = rnd.Next(2, 5);
                    }

                    bool isCrit = rnd.Next(0, 100) < 4;
                    if (isCrit)
                    {
                        damageDealt *= 2;
                        ShakeScreen(15, 10);
                        SpawnPopup("CRITICAL!", Color.Yellow, 36);
                        Log("CRITICAL HIT! You hit a weak spot!");
                    }
                    else ShakeScreen(5, 5);
                    
                    SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Red, 20);

                    int currentDiff = _playerLevel;
                    if (currentDiff > 3) currentDiff = 3;
                    
                    Mobs mobCheck = new Mobs(currentDiff);
                    int mobAction = mobCheck.ChoixActionEnnemi(currentDiff, _selectedMobId, _playerHP, (int)_currentClass, _enemyHP);
                    
                    if (mobAction == 2) 
                    {
                        damageDealt /= 2;
                        int healAmount = (int)(_enemyMaxHP * 0.20);
                        _enemyHP = Math.Min(_enemyHP + healAmount, _enemyMaxHP);
                        
                        Log($"The {_enemyName} BLOCKS! Damage halved & Healed {healAmount} HP.");
                        SpawnPopup("BLOCK", Color.Cyan);
                        SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Cyan, 10);
                    }

                    _enemyHP -= damageDealt;
                    Log($"You attacked for {damageDealt} damage!");
                    break;

                case "Block":
                    isBlocking = true;
                    Log("You brace yourself for impact (Defense UP)!");
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
                            SpawnPopup("HEAL", Color.LightGreen);
                            SpawnParticles(200, pnlBattleScene.Height/2, Color.Gold, 20);
                        }
                        else if (_currentClass == PlayerClass.DarkMage)
                        {
                            int spellDmg = 25 + (_playerLevel * 5);
                            SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Purple, 30);
                            
                            if (_enemyIsBlocking) {
                                spellDmg /= 2;
                                Log("Enemy magic resistance UP! Damage halved.");
                            }

                            _enemyHP -= spellDmg;
                            Log($"You cast FIREBALL! Dealt {spellDmg} damage.");
                        }
                        else 
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
                        SpawnPopup("NO MANA", Color.Gray);
                        // Turn is invalid, re-enable input
                        grpActions.Enabled = true;
                        return;
                    }
                    break;

                case "Item":
                    if (_potions > 0)
                    {
                        _potions--;
                        _playerHP = Math.Min(_playerHP + 50, _playerMaxHP);
                        Log("Used a Potion! +50 HP.");
                        SpawnPopup("+50 HP", Color.LightGreen);
                        SpawnParticles(200, pnlBattleScene.Height/2, Color.LightGreen, 20);
                    }
                    else
                    {
                        Log("No potions left!");
                        SpawnPopup("EMPTY", Color.Gray);
                        grpActions.Enabled = true;
                        return;
                    }
                    break;
            }

            UpdateStatsUI();
            CheckBattleStatus(isBlocking);
        }

        private void CheckBattleStatus(bool isBlocking)
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
                EnemyTurn(isBlocking);
            }
        }

        private async void EnemyTurn(bool playerBlocking)
        {
            // Attends un ti peu
            await Task.Delay(1000);

            int currentDiff = _playerLevel;
            if (currentDiff > 3) currentDiff = 3;

            Mobs mobAI = new Mobs(currentDiff);
            int actionCode = mobAI.ChoixActionEnnemi(currentDiff, _selectedMobId, _playerHP, (int)_currentClass, _enemyHP);

            _enemyIsBlocking = false;

            if (actionCode == 1) 
            {
                _enemyAnimOffset = -200; 
                await Task.Delay(200);

                int dmg = _enemyDamage;
                Random rnd = new Random();
                bool isCrit = rnd.Next(0, 100) < 4;

                if (isCrit)
                {
                    dmg *= 2;
                    ShakeScreen(20, 15);
                    SpawnPopup("OUCH!", Color.Red, 36);
                    Log("CRITICAL HIT! Enemy smashed you!");
                }
                else ShakeScreen(5, 5);

                if (playerBlocking) dmg /= 2; 

                SpawnParticles(200, pnlBattleScene.Height/2, Color.Red, 15);
                _playerHP -= dmg;
                Log($"The {_enemyName} attacks! You take {dmg} damage.");
            }
            else if (actionCode == 2) 
            {
                _enemyIsBlocking = true; 
                int regen = 5 + (currentDiff * 2);
                _enemyHP = Math.Min(_enemyHP + regen, _enemyMaxHP);
                SpawnParticles(pnlBattleScene.Width - 300, pnlBattleScene.Height/2, Color.Green, 10);
                SpawnPopup("REGEN", Color.Green);
                Log($"The {_enemyName} takes a defensive stance and regenerates {regen} HP!");
            }
            else if (actionCode == 3) 
            {
                _enemyAnimOffset = -300; 
                await Task.Delay(200);
                ShakeScreen(25, 20);

                int dmg = (int)(_enemyDamage * 1.5); 
                if (playerBlocking) dmg /= 2;

                _playerHP -= dmg;
                SpawnPopup("SPECIAL!", Color.Purple, 36);
                Log($"!!! The {_enemyName} uses a SPECIAL ABILITY! You take CRITICAL {dmg} damage!");
            }
            
            UpdateStatsUI();

            if (_playerHP <= 0)
            {
                _playerHP = 0;
                UpdateStatsUI();
                GameOver();
            }
            
            // Là je réactive les boutons
            grpActions.Enabled = true;
        }

        private void WinBattle()
        {
            _battlesWon++;

            if (_battlesWon >= 10)
            {
                ShowWinScreen();
                return;
            }
            
            int levelUpThreshold = 3; 
            
            // Nouveau popup de victoire
            SpawnPopup("VICTORY!", Color.Gold, 48);

            if (_battlesWon % levelUpThreshold == 0)
            {
                _playerLevel++;
                _playerMaxMana += 5;
                _playerHP = _playerMaxHP; 
                _playerMana = _playerMaxMana;
                Log($"LEVEL UP! You are now level {_playerLevel}. Stats increased!");
                // Popup de level up
                SpawnPopup("LEVEL UP!", Color.Cyan, 60);
                SpawnParticles(200, pnlBattleScene.Height/2, Color.Cyan, 100);
            }

            // Petit délais avant la prochaine bataille
            Task.Delay(2000).ContinueWith(t => this.Invoke((MethodInvoker)delegate {
                ShowEncounterSelection();
            }));
        }

        private void GameOver()
        {
            // Game over mon reuf
            _currentState = GameState.GameOver; // Important pour l'animation
            HideBattleUI();
            
            pnlWinScreen.Visible = true;
            pnlWinScreen.BringToFront();
            
            lblWinMessage.Text = "GAME OVER";
            lblWinMessage.ForeColor = Color.DarkRed;
            
            // On cache le bouton un instant pour l'effet de suspense...
            btnWinReturn.Text = "TRY AGAIN...";
            btnWinReturn.Visible = false;

            int w = pnlWinScreen.Width;
            int h = pnlWinScreen.Height;
            btnWinReturn.Location = new Point((w - btnWinReturn.Width) / 2, (h / 2) + 180);
            
            // Petit délai honteux avant de pouvoir quitter
            Task.Delay(2000).ContinueWith(t => this.Invoke((MethodInvoker)delegate {
                btnWinReturn.Visible = true;
            }));
            
            // J'ai enlevé ShowMainMenu() parce que ça faisait un reset trop vite à mon goût (je suis un rageux)
        }
    }
}