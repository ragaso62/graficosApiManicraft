using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using static OpenTK.Graphics.OpenGL.GL;

namespace graficosApiMinecraft.designe
{
    /// <summary>
    /// Cada metodo busca os dados na API, monta um grafico (ou uma pagina com varios)
    /// e devolve o Control pronto para o Form1 exibir.
    /// </summary>
    public static class Graficos
    {
        // Marco da pesquisa: instalacao do anticheat extra
        private static readonly DateOnly MarcoAnticheat = new(2026, 9, 6);

        // Quanto do "teto" cada serie ocupa no pico (ver grafico 1)
        private const double FolgaViolacoes = 1.2; // linha ocupa ~83% da altura
        private const double FolgaLogins = 4.0;    // colunas ficam na base (~25%)

        private static readonly string[] NomesDias = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];

        // =====================================================================
        // 1) Logins (colunas) e violacoes (linha) por dia + marco do anticheat
        // =====================================================================
        public static async Task<Control> LoginsEViolacoesAsync()
        {
            var logins = await Api.GetAsync<DiaTotal>("/api/login/por-dia");
            var violacoes = await Api.GetAsync<DiaTotal>("/api/violacoes/por-dia");

            var loginsPorDia = logins.ToDictionary(x => DateOnly.Parse(x.Dia), x => x.Total);
            var violPorDia = violacoes.ToDictionary(x => DateOnly.Parse(x.Dia), x => x.Total);

            var todas = loginsPorDia.Keys.Union(violPorDia.Keys).ToList();
            if (todas.Count == 0) throw new InvalidOperationException("A API não retornou dados.");

            // Eixo continuo: do primeiro ao ultimo dia, dias sem registro = 0
            var inicio = todas.Min();
            var fim = todas.Max();
            var dias = new List<DateOnly>();
            for (var d = inicio; d <= fim; d = d.AddDays(1)) dias.Add(d);

            var valoresLogins = dias.Select(d => loginsPorDia.GetValueOrDefault(d)).ToArray();
            var valoresViol = dias.Select(d => violPorDia.GetValueOrDefault(d)).ToArray();
            var rotulos = dias.Select(d => d.ToString("dd/MM")).ToList();

            // Esquerda (ciano) = violacoes | Direita (rosa) = logins
            var eixoViol = Tema.EixoNumerico(corTexto: Tema.Ciano, minStep: 1);
            eixoViol.MaxLimit = Math.Max(10, valoresViol.Max() * FolgaViolacoes);

            var eixoLogins = Tema.EixoNumerico(corTexto: Tema.Rosa, grade: false, minStep: 1, posicao: AxisPosition.End);
            eixoLogins.MaxLimit = Math.Max(10, valoresLogins.Max() * FolgaLogins);

            var serieLogins = new ColumnSeries<double>
            {
                Name = "Logins",
                Values = valoresLogins,
                ScalesYAt = 1,
                Fill = Tema.GradienteVertical(Tema.Laranja, Tema.Rosa),
                Stroke = null,
                Rx = 4,
                Ry = 4,
                MaxBarWidth = 22,
                AnimationsSpeed = TimeSpan.FromMilliseconds(800)
            };

            var serieViol = new LineSeries<double>
            {
                Name = "Violações",
                Values = valoresViol,
                ScalesYAt = 0,
                Stroke = Tema.GradienteLinha(Tema.Ciano, Tema.Roxo, Tema.Rosa, 3),
                Fill = Tema.GradienteArea(Tema.Roxo),
                GeometrySize = 10,
                GeometryFill = new SolidColorPaint(Tema.Fundo),
                GeometryStroke = new SolidColorPaint(Tema.Ciano) { StrokeThickness = 3 },
                LineSmoothness = 0.7,
                AnimationsSpeed = TimeSpan.FromMilliseconds(800)
            };

            var chart = Tema.NovoChart("Logins e violações por dia");
            chart.XAxes = new[] { Tema.EixoCategorias(rotulos) };
            chart.YAxes = new[] { eixoViol, eixoLogins };
            chart.Series = new ISeries[] { serieLogins, serieViol };

            // Linha vertical tracejada no dia do anticheat extra (Xi == Xj => vira uma linha)
            var indiceMarco = dias.IndexOf(MarcoAnticheat);
            if (indiceMarco >= 0)
            {
                chart.Sections = new[]
                {
                    new RectangularSection
                    {
                        Xi = indiceMarco,
                        Xj = indiceMarco,
                        Stroke = new SolidColorPaint(Tema.Laranja)
                        {
                            StrokeThickness = 2,
                            PathEffect = new DashEffect(new float[] { 8, 6 })
                        },
                        Label = "Anticheat extra",
                        LabelSize = 13,
                        LabelPaint = new SolidColorPaint(Tema.Laranja)
                    }
                };
            }

            return Pagina(chart,
                nota: "Duas escalas: violações no eixo esquerdo (ciano) e logins no direito (rosa). " +
                      "A altura de uma série não é comparável com a da outra.");
        }

        // =====================================================================
        // 2) Tempo medio de sessao por conta
        // =====================================================================
        public static async Task<Control> TempoMedioSessaoAsync()
        {
            // menor -> maior: em barras horizontais o primeiro item fica embaixo
            var dados = (await Api.GetAsync<TempoMedio>("/api/jogadores/tempo-medio-sessao"))
                        .OrderBy(x => x.MediaMinutos).ToList();
            if (dados.Count == 0) throw new InvalidOperationException("A API não retornou dados.");

            var rotulos = dados.Select(d => $"{d.Jogador} (n={d.SessoesConsideradas:0})").ToList();

            var serie = new RowSeries<double>
            {
                Name = "Minutos por sessão",
                Values = dados.Select(d => d.MediaMinutos).ToArray(),
                Fill = Tema.GradienteHorizontal(Tema.Roxo, Tema.Rosa),
                Stroke = null,
                Rx = 4,
                Ry = 4,
                MaxBarWidth = 24,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = DataLabelsPosition.End,
                AnimationsSpeed = TimeSpan.FromMilliseconds(800)
            };

            var eixoContas = Tema.EixoCategorias(rotulos);
            eixoContas.ForceStepToMin = true; // mostra o nome de todas as contas

            var chart = Tema.NovoChart("Tempo médio de sessão por conta", legenda: false);
            chart.XAxes = new[] { Tema.EixoNumerico("Minutos por sessão") };
            chart.YAxes = new[] { eixoContas };
            chart.Series = new ISeries[] { serie };

            return Pagina(chart,
                nota: "n = sessões fechadas consideradas (sessões ainda abertas ficam de fora). " +
                      "Média com n pequeno é pouco confiável. Cada barra é uma conta, não uma pessoa.");
        }

        // =====================================================================
        // 3) Mapa de calor: hora x dia da semana (acessos e violacoes)
        // =====================================================================
        public static async Task<Control> HorariosAsync()
        {
            var acessos = await HeatmapAsync(
                "Acessos por horário (sessões iniciadas)", "/api/sessoes/por-hora",
                [SKColor.Parse("#0F1F2B"), SKColor.Parse("#0E7490"), Tema.Ciano]);

            var violacoes = await HeatmapAsync(
                "Violações por horário", "/api/violacoes/por-hora",
                [SKColor.Parse("#1E1230"), Tema.Roxo, Tema.Rosa, Tema.Laranja]);

            var duplo = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Tema.Win(Tema.Fundo)
            };
            duplo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            duplo.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            duplo.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            duplo.Controls.Add(acessos, 0, 0);
            duplo.Controls.Add(violacoes, 0, 1);

            return Pagina(duplo,
                nota: "Quanto mais intensa a cor, mais ocorrências. O horário segue o fuso usado na importação dos dados " +
                      "(confira com uma sessão cujo horário você conhece).");
        }

        private static async Task<CartesianChart> HeatmapAsync(string titulo, string endpoint, SKColor[] escala)
        {
            var celulas = await Api.GetAsync<CelulaHora>(endpoint);
            var mapa = celulas.ToDictionary(c => (c.DiaSemana, c.Hora), c => c.Total);

            // Grade completa 7 x 24 (celulas sem registro = 0)
            var pontos = new List<WeightedPoint>();
            for (int dia = 1; dia <= 7; dia++)
                for (int hora = 0; hora < 24; hora++)
                    pontos.Add(new WeightedPoint(hora, dia - 1, mapa.GetValueOrDefault((dia, hora))));

            var serie = new HeatSeries<WeightedPoint>
            {
                Name = "Total",
                Values = pontos,
                HeatMap = escala
                    .Select(c => new LiveChartsCore.Drawing.LvcColor(c.Red, c.Green, c.Blue, c.Alpha))
                    .ToArray()
            };

            var horas = Enumerable.Range(0, 24).Select(h => $"{h:00}h").ToList();
            var eixoHoras = Tema.EixoCategorias(horas);
            eixoHoras.ForceStepToMin = true;

            var eixoDias = Tema.EixoCategorias(NomesDias.ToList());
            eixoDias.ForceStepToMin = true;
            eixoDias.IsInverted = true; // domingo em cima

            var chart = Tema.NovoChart(titulo, legenda: false);
            chart.XAxes = new[] { eixoHoras };
            chart.YAxes = new[] { eixoDias };
            chart.Series = new ISeries[] { serie };
            return chart;
        }

        // =====================================================================
        // 4) Dispersao: tempo jogado x violacoes (um ponto por conta)
        // =====================================================================
        public static async Task<Control> ViolacoesVsTempoAsync()
        {
            var dados = await Api.GetAsync<ViolTempo>("/api/jogadores/violacoes-vs-tempo");
            if (dados.Count == 0) throw new InvalidOperationException("A API não retornou dados.");

            // Uma serie por conta: o tooltip mostra o nome da conta sem precisar de codigo extra
            var series = dados.Select(d => (ISeries)new ScatterSeries<ObservablePoint>
            {
                Name = $"{d.Jogador} ({d.Plataforma})",
                Values = new[] { new ObservablePoint(d.MinutosJogados, d.TotalViolacoes) },
                GeometrySize = 18,
                Stroke = null,
                Fill = new SolidColorPaint((d.Plataforma == "Bedrock" ? Tema.Rosa : Tema.Ciano).WithAlpha(210)),
                DataLabelsPaint = new SolidColorPaint(Tema.TextoSuave),
                DataLabelsSize = 11,
                DataLabelsPosition = DataLabelsPosition.Top,
                DataLabelsFormatter = _ => d.TotalViolacoes >= 20 ? d.Jogador : ""
            }).ToArray();

            var chart = Tema.NovoChart("Violações × tempo jogado", legenda: false);
            chart.XAxes = new[] { Tema.EixoNumerico("Minutos jogados (sessões fechadas)") };
            chart.YAxes = new[] { Tema.EixoNumerico("Total de violações") };
            chart.XAxes = new[] { Tema.EixoNumerico("Minutos jogados (sessões fechadas)", minStep: 500) };
            chart.Series = series;

            return Pagina(chart,
                nota: "Cada ponto é uma conta. Com poucos pontos, leia como padrão observado, não como prova de relação. " +
                      "Contas ≠ pessoas: algumas pessoas jogam nas duas plataformas.",
                legenda: Legenda(("Java", Tema.Ciano), ("Bedrock", Tema.Rosa)));
        }

        // =====================================================================
        // 5) Java x Bedrock
        // =====================================================================
        public static async Task<Control> PlataformasAsync()
        {
            var dados = await Api.GetAsync<ResumoPlataforma>("/api/jogadores/por-plataforma");
            if (dados.Count == 0) throw new InvalidOperationException("A API não retornou dados.");

            double Valor(string plataforma, Func<ResumoPlataforma, double> campo) =>
                dados.Where(x => x.Plataforma == plataforma).Select(campo).FirstOrDefault();

            double PorCemSessoes(string plataforma)
            {
                var sessoes = Valor(plataforma, x => x.Sessoes);
                return sessoes == 0 ? 0 : Math.Round(Valor(plataforma, x => x.Violacoes) / sessoes * 100, 1);
            }

            var grade = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Tema.Win(Tema.Fundo)
            };
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grade.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grade.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            grade.Controls.Add(BarrasPlataforma("Contas", Valor("Java", x => x.Contas), Valor("Bedrock", x => x.Contas)), 0, 0);
            grade.Controls.Add(BarrasPlataforma("Sessões", Valor("Java", x => x.Sessoes), Valor("Bedrock", x => x.Sessoes)), 1, 0);
            grade.Controls.Add(BarrasPlataforma("Violações", Valor("Java", x => x.Violacoes), Valor("Bedrock", x => x.Violacoes)), 0, 1);
            grade.Controls.Add(BarrasPlataforma("Violações por 100 sessões", PorCemSessoes("Java"), PorCemSessoes("Bedrock")), 1, 1);

            return Pagina(grade,
                nota: "A plataforma é deduzida do UUID (Bedrock via Floodgate = UUID com prefixo zerado). " +
                      "Contas ≠ pessoas: uma pessoa pode ter conta Java e Bedrock, então os grupos não são independentes.",
                legenda: Legenda(("Java", Tema.Ciano), ("Bedrock", Tema.Rosa)));
        }

        private static CartesianChart BarrasPlataforma(string titulo, double java, double bedrock)
        {
            ColumnSeries<double> Barra(string nome, double valor, SKColor a, SKColor b) => new()
            {
                Name = nome,
                Values = new[] { valor },
                Fill = Tema.GradienteVertical(a, b),
                Stroke = null,
                Rx = 6,
                Ry = 6,
                MaxBarWidth = 70,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 14,
                DataLabelsPosition = DataLabelsPosition.End,
                AnimationsSpeed = TimeSpan.FromMilliseconds(800)
            };

            var eixoX = Tema.EixoCategorias(new List<string> { "" });
            eixoX.IsVisible = false;

            var chart = Tema.NovoChart(titulo, legenda: false);
            chart.XAxes = new[] { eixoX };
            chart.YAxes = new[] { Tema.EixoNumerico(minStep: 1) };
            chart.Series = new ISeries[]
            {
                Barra("Java", java, Tema.Ciano, Tema.Roxo),
                Barra("Bedrock", bedrock, Tema.Rosa, Tema.Laranja)
            };
            return chart;
        }

        // =====================================================================
        // Ajudantes de layout: pagina = conteudo + (legenda) + (nota)
        // =====================================================================
        private static Control Pagina(Control conteudo, string? nota = null, Control? legenda = null)
        {
            var t = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                BackColor = Tema.Win(Tema.Fundo)
            };
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            conteudo.Dock = DockStyle.Fill;
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.Controls.Add(conteudo, 0, 0);

            if (legenda != null)
            {
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                t.Controls.Add(legenda, 0, t.RowStyles.Count - 1);
            }

            if (nota != null)
            {
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
                t.Controls.Add(Tema.Info(nota), 0, t.RowStyles.Count - 1);
            }

            return t;
        }

        private static Control Legenda(params (string Texto, SKColor Cor)[] itens)
        {
            var f = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                BackColor = Tema.Win(Tema.Fundo)
            };

            foreach (var (texto, cor) in itens)
            {
                f.Controls.Add(new Label
                {
                    Text = "●  " + texto,
                    AutoSize = true,
                    ForeColor = Tema.Win(cor),
                    Font = new Font("Segoe UI", 10.5f),
                    Margin = new Padding(20, 4, 10, 0)
                });
            }

            return f;
        }
    }
}