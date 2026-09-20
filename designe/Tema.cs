using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace graficosApiMinecraft.designe
{
    /// <summary>
    /// Paleta e estilos do dashboard (tema escuro/neon).
    /// Todos os graficos usam isto, entao o visual muda em um lugar so.
    /// </summary>
    public static class Tema
    {
        // ===== Paleta =====
        public static readonly SKColor Fundo = SKColor.Parse("#0B0F14");
        public static readonly SKColor Painel = SKColor.Parse("#111722");
        public static readonly SKColor Grade = SKColor.Parse("#1B2230");
        public static readonly SKColor TextoSuave = SKColor.Parse("#8B93A7");
        public static readonly SKColor TextoClaro = SKColor.Parse("#E6E9F2");
        public static readonly SKColor Laranja = SKColor.Parse("#FF9A1F");
        public static readonly SKColor Rosa = SKColor.Parse("#FF2D95");
        public static readonly SKColor Roxo = SKColor.Parse("#8A2BE2");
        public static readonly SKColor Ciano = SKColor.Parse("#22D3EE");

        /// <summary>Converte cor do SkiaSharp para cor do WinForms.</summary>
        public static System.Drawing.Color Win(SKColor c) =>
            System.Drawing.Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);

        // ===== Gradientes =====
        public static LinearGradientPaint GradienteLinha(SKColor a, SKColor b, SKColor c, float espessura) =>
            new(new[] { a, b, c }, new SKPoint(0, 0.5f), new SKPoint(1, 0.5f))
            {
                StrokeThickness = espessura
            };

        public static LinearGradientPaint GradienteArea(SKColor topo) =>
            new(new[] { topo.WithAlpha(110), topo.WithAlpha(0) },
                new SKPoint(0.5f, 0), new SKPoint(0.5f, 1));

        public static LinearGradientPaint GradienteVertical(SKColor topo, SKColor baixo) =>
            new(new[] { topo, baixo }, new SKPoint(0.5f, 0), new SKPoint(0.5f, 1));

        public static LinearGradientPaint GradienteHorizontal(SKColor esquerda, SKColor direita) =>
            new(new[] { esquerda, direita }, new SKPoint(0, 0.5f), new SKPoint(1, 0.5f));

        // ===== Eixos =====

        /// <summary>Eixo de categorias (dias, nomes de jogadores, horas...).</summary>
        public static Axis EixoCategorias(IList<string> rotulos) =>
            new()
            {
                Labels = rotulos,
                MinStep = 1,
                LabelsPaint = new SolidColorPaint(TextoSuave),
                TextSize = 13,
                SeparatorsPaint = null,
                TicksPaint = null,
                SubticksPaint = null
            };

        /// <summary>Eixo numerico comecando em zero.</summary>
        public static Axis EixoNumerico(
            string? nome = null,
            SKColor? corTexto = null,
            bool grade = true,
            double minStep = 0,
            AxisPosition posicao = AxisPosition.Start) =>
            new()
            {
                MinLimit = 0,
                MinStep = minStep,
                Position = posicao,
                Name = nome,
                NamePaint = nome is null ? null : new SolidColorPaint(TextoSuave),
                NameTextSize = 13,
                LabelsPaint = new SolidColorPaint(corTexto ?? TextoSuave),
                TextSize = 13,
                SeparatorsPaint = grade ? new SolidColorPaint(Grade) { StrokeThickness = 1 } : null
            };

        // ===== Grafico base =====

        /// <summary>Grafico cartesiano ja com titulo, legenda e tooltip no tema escuro.</summary>
        public static CartesianChart NovoChart(string titulo, bool legenda = true) =>
            new()
            {
                Dock = DockStyle.Fill,
                BackColor = Win(Fundo),

                Title = new LabelVisual
                {
                    Text = titulo,
                    TextSize = 18,
                    Paint = new SolidColorPaint(SKColors.White),
                    Padding = new LiveChartsCore.Drawing.Padding(15)
                },

                LegendPosition = legenda ? LegendPosition.Top : LegendPosition.Hidden,
                LegendTextPaint = new SolidColorPaint(TextoSuave),
                LegendTextSize = 13,

                TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#151B26")),
                TooltipTextPaint = new SolidColorPaint(SKColors.White),
                TooltipTextSize = 14
            };

        /// <summary>Faixa de texto explicativo (usada acima de alguns graficos).</summary>
        public static Label Info(string texto) =>
            new()
            {
                Dock = DockStyle.Fill,
                Text = texto,
                ForeColor = Win(TextoSuave),
                BackColor = Win(Fundo),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f)
            };
    }
}