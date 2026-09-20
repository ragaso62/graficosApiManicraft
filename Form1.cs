using graficosApiMinecraft.designe;

namespace graficosApiMinecraft
{
    public partial class Form1 : Form
    {
        private readonly Panel areaConteudo = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Tema.Win(Tema.Fundo)
        };

        private readonly List<Button> botoes = new();
        private Button? botaoAtivo;
        private int versaoAbertura; // evita que uma tela lenta sobrescreva outra mais recente

        public Form1()
        {
            InitializeComponent();

            Text = "Dashboard - Minecraft";
            BackColor = Tema.Win(Tema.Fundo);
            ClientSize = new Size(1250, 760);
            StartPosition = FormStartPosition.CenterScreen;

            MontarLayout();
            Load += async (_, _) => await AbrirAsync(botoes[0], Graficos.LoginsEViolacoesAsync);
        }

        private void MontarLayout()
        {
            var raiz = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Tema.Win(Tema.Fundo)
            };
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            raiz.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Tema.Win(Tema.Painel),
                Padding = new Padding(10, 20, 10, 10)
            };

            menu.Controls.Add(new Label
            {
                Text = "GRIM DASHBOARD",
                AutoSize = true,
                ForeColor = Tema.Win(Tema.Rosa),
                Font = new Font("Segoe UI Semibold", 13f),
                Margin = new Padding(6, 0, 0, 24)
            });

            var telas = new (string Titulo, Func<Task<Control>> Construtor)[]
            {
                ("Logins e violações",        Graficos.LoginsEViolacoesAsync),
                ("Tempo de sessão",           Graficos.TempoMedioSessaoAsync),
                ("Horários de pico",          Graficos.HorariosAsync),
                ("Violações × tempo jogado",  Graficos.ViolacoesVsTempoAsync),
                ("Java × Bedrock",            Graficos.PlataformasAsync),
            };

            foreach (var (titulo, construtor) in telas)
            {
                var botao = CriarBotao(titulo, construtor);
                botoes.Add(botao);
                menu.Controls.Add(botao);
            }

            raiz.Controls.Add(menu, 0, 0);
            raiz.Controls.Add(areaConteudo, 1, 0);
            Controls.Add(raiz);
        }

        private Button CriarBotao(string texto, Func<Task<Control>> construtor)
        {
            var botao = new Button
            {
                Text = texto,
                Width = 205,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Tema.Win(Tema.Painel),
                ForeColor = Tema.Win(Tema.TextoSuave),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 6),
                Font = new Font("Segoe UI", 10.5f),
                Cursor = Cursors.Hand
            };
            botao.FlatAppearance.BorderSize = 0;
            botao.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(28, 35, 48);
            botao.Click += async (_, _) => await AbrirAsync(botao, construtor);
            return botao;
        }

        private void MarcarAtivo(Button botao)
        {
            if (botaoAtivo != null)
            {
                botaoAtivo.BackColor = Tema.Win(Tema.Painel);
                botaoAtivo.ForeColor = Tema.Win(Tema.TextoSuave);
            }

            botaoAtivo = botao;
            botao.BackColor = Tema.Win(Tema.Grade);
            botao.ForeColor = Tema.Win(Tema.TextoClaro);
        }

        private void LimparConteudo()
        {
            foreach (var c in areaConteudo.Controls.Cast<Control>().ToList())
            {
                areaConteudo.Controls.Remove(c);
                c.Dispose();
            }
        }

        private async Task AbrirAsync(Button botao, Func<Task<Control>> construtor)
        {
            int minhaVersao = ++versaoAbertura;

            MarcarAtivo(botao);
            LimparConteudo();

            var aviso = Tema.Info("Carregando...");
            areaConteudo.Controls.Add(aviso);

            try
            {
                var tela = await construtor();

                if (minhaVersao != versaoAbertura)
                {
                    tela.Dispose(); // o usuario ja abriu outra tela
                    return;
                }

                LimparConteudo();
                tela.Dock = DockStyle.Fill;
                areaConteudo.Controls.Add(tela);
            }
            catch (Exception ex)
            {
                if (minhaVersao != versaoAbertura) return;
                aviso.Text = "Não foi possível carregar os dados.\n\n" +
                             "O backend (Spring Boot) está rodando em http://localhost:8080?\n\n" + ex.Message;
            }
        }
    }
}
