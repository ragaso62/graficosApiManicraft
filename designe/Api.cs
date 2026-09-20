using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace graficosApiMinecraft.designe
{
    // ===== Modelos: um por endpoint. Os nomes em [JsonPropertyName] sao as colunas devolvidas pela API. =====

    /// <summary>/api/login/por-dia e /api/violacoes/por-dia</summary>
    public record DiaTotal(string Dia, double Total);

    /// <summary>/api/jogadores/tempo-medio-sessao</summary>
    public record TempoMedio(
        string Jogador,
        [property: JsonPropertyName("media_minutos")] double MediaMinutos,
        [property: JsonPropertyName("sessoes_consideradas")] double SessoesConsideradas);

    /// <summary>/api/sessoes/por-hora e /api/violacoes/por-hora (dia_semana: 1 = domingo ... 7 = sabado)</summary>
    public record CelulaHora(
        [property: JsonPropertyName("dia_semana")] int DiaSemana,
        int Hora,
        double Total);

    /// <summary>/api/jogadores/violacoes-vs-tempo</summary>
    public record ViolTempo(
        string Jogador,
        string? Plataforma,
        [property: JsonPropertyName("minutos_jogados")] double MinutosJogados,
        [property: JsonPropertyName("total_violacoes")] double TotalViolacoes);

    /// <summary>/api/jogadores/por-plataforma</summary>
    public record ResumoPlataforma(string? Plataforma, double Contas, double Sessoes, double Violacoes);

    public static class Api
    {
        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri("http://localhost:8080"),
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static async Task<List<T>> GetAsync<T>(string caminho) =>
            await Http.GetFromJsonAsync<List<T>>(caminho) ?? new List<T>();
    }
}