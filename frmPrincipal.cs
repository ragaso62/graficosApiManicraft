using System.Net.Http.Json;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using System.Drawing.Drawing2D;

namespace graficosApiMinecraft
{
    public partial class frmPrincipal : Form
    {
        // Cores do projeto
        private readonly Color Fundo = Color.FromArgb(11, 15, 20);
        private readonly Color Card = Color.FromArgb(17, 24, 32);
        private readonly Color CardHover = Color.FromArgb(23, 32, 42);

        private readonly Color Azul = Color.FromArgb(41, 112, 194);
        private readonly Color Ciano = Color.FromArgb(0, 217, 255);
        private readonly Color Rosa = Color.FromArgb(255, 45, 141);
        private readonly Color Roxo = Color.FromArgb(130, 70, 163);
        private readonly Color Verde = Color.FromArgb(88, 194, 85);

        private readonly Color Texto = Color.FromArgb(245, 245, 245);
        private readonly Color TextoSecundario = Color.FromArgb(145, 155, 170);

        public frmPrincipal()
        {
            InitializeComponent();

            CriarHome();
        }

        private void CriarHome()
        {
            BackColor = Fundo;
            Text = "Dashboard - Minecraft";
            MinimumSize = new Size(1000, 600);

            // =====================================================
            // SIDEBAR
            // =====================================================

            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(9, 13, 18)
            };

            Controls.Add(sidebar);

            Label logo = new Label
            {
                Text = "MINECRAFT\nANALYTICS",
                ForeColor = Texto,
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                Location = new Point(25, 30),
                Size = new Size(180, 60)
            };

            sidebar.Controls.Add(logo);

            Label menuTitulo = new Label
            {
                Text = "MENU",
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(25, 120),
                AutoSize = true
            };

            sidebar.Controls.Add(menuTitulo);

            Button btnInicio = CriarBotaoMenu("▣   Início", true);
            btnInicio.Location = new Point(15, 150);

            Button btnGraficos = CriarBotaoMenu("▥   Gráficos", false);
            btnGraficos.Location = new Point(15, 200);

            Button btnJogadores = CriarBotaoMenu("◉   Jogadores", false);
            btnJogadores.Location = new Point(15, 250);

            Button btnSessoes = CriarBotaoMenu("◉   Sessões", false);
            btnSessoes.Location = new Point(15, 300);

            sidebar.Controls.Add(btnInicio);
            sidebar.Controls.Add(btnGraficos);
            sidebar.Controls.Add(btnJogadores);
            sidebar.Controls.Add(btnSessoes);

            // =====================================================
            // ÁREA PRINCIPAL
            // =====================================================

            Panel conteudo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Fundo,
                Padding = new Padding(35)
            };

            Controls.Add(conteudo);
            conteudo.BringToFront();

            // Cabeçalho
            Label titulo = new Label
            {
                Text = "Visão geral",
                ForeColor = Texto,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                Location = new Point(35, 30),
                AutoSize = true
            };

            conteudo.Controls.Add(titulo);

            Label subtitulo = new Label
            {
                Text = "Monitoramento da comunidade Minecraft",
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 10),
                Location = new Point(38, 72),
                AutoSize = true
            };

            conteudo.Controls.Add(subtitulo);

            // Status
            Label status = new Label
            {
                Text = "●  SISTEMA ONLINE",
                ForeColor = Verde,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(0, 40)
            };

            conteudo.Controls.Add(status);

            // =====================================================
            // CARDS DE ESTATÍSTICAS
            // =====================================================

            RoundedPanel cardPlayers = CriarCard(
                "JOGADORES",
                "7",
                "Registrados",
                Ciano
            );

            cardPlayers.Location = new Point(35, 125);

            RoundedPanel cardSessions = CriarCard(
                "SESSÕES",
                "67",
                "Registradas",
                Azul
            );

            cardSessions.Location = new Point(300, 125);

            RoundedPanel cardViolations = CriarCard(
                "VIOLAÇÕES",
                "3.769",
                "Detectadas pelo GrimAC",
                Rosa
            );

            cardViolations.Location = new Point(565, 125);

            conteudo.Controls.Add(cardPlayers);
            conteudo.Controls.Add(cardSessions);
            conteudo.Controls.Add(cardViolations);

            // =====================================================
            // CARD ATIVIDADE
            // =====================================================

            RoundedPanel atividade = CriarCardBase();
            atividade.Location = new Point(35, 285);
            atividade.Size = new Size(430, 230);

            Label atividadeTitulo = CriarTituloCard("ATIVIDADE RECENTE");
            atividadeTitulo.Location = new Point(22, 20);

            atividade.Controls.Add(atividadeTitulo);

            Label pico = new Label
            {
                Text = "06/09",
                ForeColor = Rosa,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                Location = new Point(22, 60),
                AutoSize = true
            };

            atividade.Controls.Add(pico);

            Label picoDescricao = new Label
            {
                Text = "Maior pico de violações",
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 10),
                Location = new Point(25, 105),
                AutoSize = true
            };

            atividade.Controls.Add(picoDescricao);

            Label picoValor = new Label
            {
                Text = "1.060",
                ForeColor = Texto,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(22, 145),
                AutoSize = true
            };

            atividade.Controls.Add(picoValor);

            Label picoUnidade = new Label
            {
                Text = "violações registradas",
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 9),
                Location = new Point(90, 153),
                AutoSize = true
            };

            atividade.Controls.Add(picoUnidade);

            conteudo.Controls.Add(atividade);

            // =====================================================
            // CARD SISTEMA
            // =====================================================

            RoundedPanel sistema = CriarCardBase();
            sistema.Location = new Point(490, 285);
            sistema.Size = new Size(430, 230);

            Label sistemaTitulo = CriarTituloCard("STATUS DO SISTEMA");
            sistemaTitulo.Location = new Point(22, 20);

            sistema.Controls.Add(sistemaTitulo);

            AdicionarStatus(sistema, "●", "Banco de dados", Verde, 65);
            AdicionarStatus(sistema, "●", "API REST", Verde, 105);
            AdicionarStatus(sistema, "●", "Minecraft", Verde, 145);

            conteudo.Controls.Add(sistema);
        }

        private Button CriarBotaoMenu(string texto, bool ativo)
        {
            Button botao = new Button
            {
                Text = texto,
                Size = new Size(190, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = ativo ? Color.FromArgb(25, 40, 55) : Color.Transparent,
                ForeColor = ativo ? Ciano : TextoSecundario,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            botao.FlatAppearance.BorderSize = 0;

            return botao;
        }

        private RoundedPanel CriarCard(
            string titulo,
            string valor,
            string descricao,
            Color destaque)
        {
            RoundedPanel card = CriarCardBase();

            card.Size = new Size(240, 130);

            Label lblTitulo = new Label
            {
                Text = titulo,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(20, 18),
                AutoSize = true
            };

            Label lblValor = new Label
            {
                Text = valor,
                ForeColor = destaque,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                Location = new Point(20, 40),
                AutoSize = true
            };

            Label lblDescricao = new Label
            {
                Text = descricao,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 8),
                Location = new Point(22, 95),
                AutoSize = true
            };

            card.Controls.Add(lblTitulo);
            card.Controls.Add(lblValor);
            card.Controls.Add(lblDescricao);

            return card;
        }

        private RoundedPanel CriarCardBase()
        {
            return new RoundedPanel
            {
                Size = new Size(300, 150),
                BackColor = Card
            };
        }

        private Label CriarTituloCard(string texto)
        {
            return new Label
            {
                Text = texto,
                ForeColor = TextoSecundario,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true
            };
        }

        private void AdicionarStatus(
            Panel painel,
            string simbolo,
            string texto,
            Color cor,
            int y)
        {
            Label status = new Label
            {
                Text = simbolo,
                ForeColor = cor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(22, y),
                AutoSize = true
            };

            Label descricao = new Label
            {
                Text = texto,
                ForeColor = Texto,
                Font = new Font("Segoe UI", 10),
                Location = new Point(45, y + 1),
                AutoSize = true
            };

            painel.Controls.Add(status);
            painel.Controls.Add(descricao);
        }
    }

    public class RoundedPanel : Panel
    {
        public RoundedPanel()
        {
            Resize += (s, e) => AtualizarRegiao();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using Pen pen = new Pen(Color.FromArgb(30, 40, 52), 1);

            Rectangle rect = new Rectangle(
                0,
                0,
                Width - 1,
                Height - 1
            );

            using GraphicsPath path = CriarPath(rect, 12);

            e.Graphics.DrawPath(pen, path);
        }

        private void AtualizarRegiao()
        {
            if (Width <= 0 || Height <= 0)
                return;

            Rectangle rect = new Rectangle(
                0,
                0,
                Width,
                Height
            );

            using GraphicsPath path = CriarPath(rect, 12);

            Region = new Region(path);
        }

        private GraphicsPath CriarPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            int diametro = radius * 2;

            path.AddArc(rect.X, rect.Y, diametro, diametro, 180, 90);
            path.AddArc(
                rect.Right - diametro,
                rect.Y,
                diametro,
                diametro,
                270,
                90
            );

            path.AddArc(
                rect.Right - diametro,
                rect.Bottom - diametro,
                diametro,
                diametro,
                0,
                90
            );

            path.AddArc(
                rect.X,
                rect.Bottom - diametro,
                diametro,
                diametro,
                90,
                90
            );

            path.CloseFigure();

            return path;
        }
    }
}