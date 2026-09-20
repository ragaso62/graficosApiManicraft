# Padrões de acesso e violações registradas por um anticheat em um servidor Minecraft: análise antes e depois de uma intervenção de moderação

Projeto final da disciplina de Metodologia Científica — Tecnologia em Banco de Dados.

---

## 1. Título e contexto

### Pergunta de pesquisa

> Como o padrão de acesso dos jogadores (frequência de login, duração de sessão, horário e plataforma) se relaciona com as violações registradas pelo anticheat **GrimAC** em um servidor Minecraft pequeno, e como esse cenário mudou após a intervenção de moderação de **06/09/2026**?

### Hipóteses testadas

| # | Hipótese |
|---|----------|
| H1 | O número de violações diminuiu após a instalação de um anticheat adicional em 06/09/2026. |
| H2 | Contas com mais violações têm sessões de duração diferente das demais. |
| H3 | As violações são apenas consequência de jogar mais (volume de jogo). |
| H4 | O acesso se concentra em horários específicos (fins de semana), e as violações acompanham esse padrão. |
| H5 | Há diferença nas violações registradas entre contas Java e Bedrock. |

### Justificativa

Servidores comunitários pequenos costumam ser moderados "a olho", sem dados que mostrem se as ferramentas de detecção e as regras adotadas funcionam. A trapaça em jogos online prejudica a experiência dos demais jogadores e exige respostas da administração (ver referência [8]). Este trabalho usa os registros que o próprio servidor já produz para responder, de forma descritiva e verificável, o que os dados mostram e o que eles **não** permitem afirmar.

### Escopo e intervenções

É um **estudo de caso descritivo** de um único servidor, sem grupo de controle. Ocorreram duas intervenções: (1) instalação de um anticheat adicional em **06/09/2026** e (2) uma regra de expulsão automática após 30 violações (VL), adotada em data posterior, cujo dia exato não foi registrado.

---

## 2. Fonte de dados

| Base | Origem | Conteúdo usado |
|------|--------|----------------|
| `history.v1.db` (SQLite) | Plugin **GrimAC**, no servidor Paper do autor | Jogadores, sessões (entrada/saída), violações (nível, tipo de verificação, instante) |
| `authme.db` (SQLite) | Plugin **AuthMe**, no mesmo servidor | Apenas data de registro por nome de jogador |
| `ops.json` | Servidor Paper | Contas com permissão de operador |

**Período:** sessões e logins de 29/08/2026 a 18/09/2026; violações de 30/08/2026 a 16/09/2026.
**Volume no banco analisado:** 13 contas (6 Java e 7 Bedrock), 323 sessões e 11 141 violações.

**Origem e licenciamento.** Os dados foram gerados pelo servidor do próprio autor; não vêm de terceiros nem de fonte pública. Os bancos brutos **não** estão neste repositório. As ferramentas utilizadas têm licenças próprias, listadas nas referências (consultar a licença de cada projeto em seu repositório oficial).

**Privacidade.** As colunas sensíveis do AuthMe (senha, IP de registro e de login) **não foram importadas**. Os jogadores são identificados apenas por apelido de jogo, e nenhuma conta é associada a uma pessoa real neste relatório. *(Confirmar: os participantes sabem que seus registros de jogo foram usados neste trabalho.)*

---

## 3. Metodologia

### 3.1 Pipeline

```
GrimAC/AuthMe (SQLite) ──► importador Java ──► MySQL ──► API Spring Boot (JdbcTemplate) ──► dashboard C# (WinForms + LiveCharts2)
```

O relatório é reproduzível: cada número e gráfico abaixo vem de um endpoint da API, exibido no dashboard.

### 3.2 Limpeza e transformação

- **Identificadores:** UUIDs armazenados como BLOB foram convertidos para texto.
- **Datas:** instantes em época (epoch) foram convertidos para data e hora.
- **Plataforma (Java/Bedrock):** deduzida do UUID. Jogadores Bedrock que entram via Geyser/Floodgate recebem UUID com os 64 bits mais altos zerados; os demais foram classificados como Java. O critério foi conferido manualmente contra os jogadores conhecidos.
- **Dados sensíveis:** colunas de senha e IP descartadas na importação.
- **Sessões abertas:** ficam de fora das médias de duração (só sessões com saída registrada).
- **Dias sem registro:** preenchidos com zero nos gráficos diários, para o eixo do tempo ser contínuo.
- **Agregações:** sessões e violações foram somadas em subconsultas separadas antes de serem ligadas ao jogador. Ligar as duas tabelas no mesmo `JOIN` repetiria cada sessão por violação e inflaria os totais.

### 3.3 Análises

1. Logins (sessões iniciadas) e violações **por dia**, com marco em 06/09.
2. **Tempo médio de sessão** por conta, com o número de sessões (n).
3. **Mapa de calor** de acessos e de violações por dia da semana × hora.
4. **Dispersão** de violações × tempo total jogado, por conta.
5. **Java × Bedrock**: contas, sessões, violações e violações por 100 sessões.

Todas são análises **descritivas**; não foram aplicados testes estatísticos, pois o número de contas (13) não os sustentaria.

### 3.4 Como reproduzir

1. Criar o banco MySQL com o esquema em `database/` e rodar o importador (`importarDados.java`) apontando para os bancos SQLite do servidor.
2. Subir a API: `cd backend && mvn spring-boot:run` (porta 8080).
3. Abrir o projeto do dashboard no Visual Studio e executar.

| Endpoint | Uso |
|----------|-----|
| `/api/login/por-dia` | Logins por dia |
| `/api/violacoes/por-dia` | Violações por dia |
| `/api/jogadores/tempo-medio-sessao` | Tempo médio de sessão |
| `/api/sessoes/por-hora`, `/api/violacoes/por-hora` | Mapas de calor |
| `/api/jogadores/violacoes-vs-tempo` | Dispersão |
| `/api/jogadores/por-plataforma` | Java × Bedrock |

### 3.5 Validação da importação (clareza dos dados)

Depois de cada carga, as contagens do MySQL foram comparadas com as da origem (reconciliação origem × destino). Essa conferência revelou um problema na primeira versão do importador:

- **Sintoma:** a origem (`grim_violations`) tinha **11 141** violações (30/08 a 16/09/2026), mas o MySQL só tinha **2 509**.
- **Causa:** a tabela `violacoes` tinha uma chave única sobre (sessão, tipo de verificação, instante) e o importador usava `ON DUPLICATE KEY UPDATE`. Como o instante é guardado com precisão de segundos, violações do mesmo tipo, na mesma sessão e no mesmo segundo eram fundidas em uma só linha.
- **Correção:** cada violação passou a guardar o **identificador original do Grim** (`grim_id`), usado como chave única. Assim a importação continua repetível (rodar duas vezes não duplica) e nenhum registro é agrupado.
- **Resultado após a correção:** origem = 11 141 violações, destino = 11 141 (nenhuma violação ficou sem jogador correspondente). Além disso, `COUNT(*)` = `COUNT(DISTINCT grim_id)` = 11 141 e não há `grim_id` nulo (a coluna é `NOT NULL`), ou seja, cada linha do destino corresponde a exatamente um registro da origem.
- **Repetibilidade:** o importador foi executado uma segunda vez e a contagem permaneceu em 11 141, isto é, reexecutar a carga atualiza os registros existentes e não os duplica.
- **Consequência para a análise:** todos os gráficos e números deste relatório foram refeitos com os dados corrigidos; os valores da primeira importação foram descartados.

Também foi conferido o **fuso horário**: os horários da origem, convertidos para UTC−3 (Brasília), coincidem com os do MySQL.

---

## 4. Resultados e limitações

> Resultados calculados com os 11 141 registros corrigidos (ver 3.5). Valores com "≈" foram lidos nos gráficos do dashboard; o valor exato aparece ao passar o mouse sobre o ponto ou a barra.

### 4.1 Logins e violações por dia

![Logins e violações por dia](docs/img/LoginsViolacoes.png)

- Há dois picos de violações: **≈2 500 em 01/09** e **5 187 em 06/09**. Juntos, os dois dias concentram cerca de dois terços de todas as violações do período.
- Depois de 06/09 os valores ficam na casa das centenas (≈540 em 07/09, ≈300 em 08/09), com uma pequena alta em 12/09 (≈450), e chegam perto de zero a partir de meados de setembro.
- Os logins também tiveram o máximo em 06/09 (69) e voltaram ao patamar de ≈10 a 30 por dia nos dias seguintes.
- Cada "violação" é um registro (um alerta) do Grim. Um único episódio pode gerar centenas de registros, então os picos indicam episódios intensos, e não milhares de trapaças distintas.

**O que se pode dizer:** a queda é *consistente com* um efeito do anticheat adicional.
**O que não se pode dizer:** que o anticheat causou a queda. O pico de 01/09 também desapareceu sem nenhuma intervenção; a maior parte do volume estava concentrada em poucas contas; os logins voltaram ao patamar habitual junto com as violações; e o instrumento de medida (o próprio Grim) pode ter mudado de sensibilidade quando o segundo anticheat entrou.

### 4.2 Tempo médio de sessão

![Tempo médio de sessão por conta](docs/img/TempoSessao.png)

- A maior média (≈66 min) é de uma conta Bedrock **sem** violações. As duas contas Java com mais violações ficam em ≈31–37 min, próximas de contas com muito menos violações (≈22–24 min).
- Não há diferença clara de duração de sessão entre contas com muitas e com poucas violações (H2 **não sustentada**).
- Contas com n = 1 ou 2 não permitem conclusão. Algumas contas têm sessões de ≈30 s, que provavelmente são reconexões ou logins que falharam, e não jogo real.

### 4.3 Horários de pico

![Mapa de calor de acessos e violações](docs/img/HorarioPico.png)

- Os **acessos** se concentram nos fins de semana, de ≈14h às 22h, com máximo perto das 19h (H4, primeira parte, **sustentada**).
- As **violações** não seguem esse padrão: as regiões mais intensas (domingo de 17h a 20h, com máximo às 18h, e terça-feira, de manhã e por volta das 13h) coincidem com os dias dos dois picos isolados (01/09 foi terça e 06/09 foi domingo). Isso indica **dois eventos pontuais**, e não um horário recorrente de trapaça. O sábado, com muito acesso, quase não tem violações.
- Com cerca de três semanas de dados, cada dia da semana aparece só duas ou três vezes, então um único evento domina a escala de cores.

### 4.4 Violações × tempo jogado

![Dispersão de violações por tempo jogado](docs/img/ViolacaoXTempo.png)

- As contas com mais tempo de jogo não são as com mais violações: uma conta Bedrock com ≈2 900 min tem praticamente nenhuma violação; uma conta Java com ≈1 900 min tem ≈5 650; e outra conta Java, com ≈4 100 min (o maior tempo de todas), tem ≈4 200, menos que a anterior.
- As violações se concentram em poucas contas, todas Java: as duas primeiras respondem por cerca de 89% do total (≈9 900 de 11 141). Uma conta do próprio administrador também aparece com ≈745 violações, o que ilustra que a violação **registrada** não equivale a trapaça (há falsos positivos e testes).
- H3 **não sustentada**: o tempo jogado, isoladamente, não explica o padrão.

### 4.5 Java × Bedrock

![Comparação Java × Bedrock](docs/img/JavaXBedrock.png)

| | Java | Bedrock |
|---|---|---|
| Contas | 6 | 7 |
| Sessões | 258 | 65 |
| Violações | 11 141 | 0 |
| Violações por 100 sessões | ≈4 318 | 0 |

- O Java tem cerca de 4× mais sessões, mas mesmo **normalizando por sessão** a diferença permanece: se as violações fossem proporcionais ao volume de sessões, o Bedrock teria algo em torno de 2 800, e não zero.
- **Ressalvas:** (a) o Grim está configurado igualmente para todos, mas pode se comportar de outra forma com jogadores via Geyser, então o resultado deve ser lido como "**0 violações registradas**", e não como "não trapaceiam"; (b) quase todas as violações vêm de duas contas Java; (c) parte das pessoas tem conta nas duas plataformas, então os grupos não são independentes.

### 4.6 Síntese

| Hipótese | Resultado | Conclusão permitida |
|---|---|---|
| H1 — queda após 06/09 | Queda expressiva observada | Consistente com o anticheat; causalidade **não** demonstrada |
| H2 — sessões diferentes | Sem diferença clara | Não sustentada |
| H3 — só volume de jogo | Volume não explica | Não sustentada |
| H4 — acesso e violação por horário | Acesso: fins de semana. Violação: dois eventos pontuais | Acesso sustentado; violação sem padrão semanal |
| H5 — Java × Bedrock | ≈4 318 × 0 por 100 sessões | Diferença nos registros; causa **não** identificada |

### 4.7 Limitações

- **Amostra pequena:** 13 contas e cerca de três semanas. Nenhum teste estatístico é apropriado, e os resultados não se generalizam a outros servidores.
- **Conta ≠ pessoa:** algumas pessoas jogam com duas contas (Java e Bedrock). Isso divide os totais por jogador e torna os grupos Java/Bedrock não independentes. Além disso, o servidor roda com `online-mode=false`, o que não garante a identidade de quem usa um apelido.
- **Sem grupo de controle** e com **duas intervenções** (anticheat adicional e expulsão automática aos 30 VL) que este estudo não consegue separar.
- **O instrumento é o próprio Grim:** violações registradas não são o mesmo que trapaça (há falsos positivos), e um anticheat adicional pode impedir que o Grim registre violações que continuariam acontecendo.
- **Unidade de contagem:** cada violação é um alerta do Grim, e um único episódio pode gerar centenas deles. Os totais medem a intensidade dos alertas, e não o número de incidentes ou de trapaceiros.
- **Bedrock com 0 violações registradas** não prova ausência de trapaça (ver 4.5).
- **"Login" = sessão iniciada**, incluindo reconexões e sessões de poucos segundos, e não jogadores distintos.
- **Fuso horário:** os instantes foram convertidos para UTC−3 (Brasília) na importação, e a conversão foi conferida comparando os horários da origem e do destino.
- **Duas escalas** no gráfico diário: a altura das colunas (logins) e da linha (violações) não é comparável.

### 4.8 Trabalhos futuros

Comparar cada conta **antes × depois** de 06/09; agrupar contas por pessoa; contar **incidentes** (por exemplo, violações agrupadas por conta e minuto) além de alertas; analisar violações por **tipo de verificação** (o endpoint já existe na API); e, se possível, obter um período de comparação sem intervenção.

---

## 5. Referências

<!-- Conferir cada URL, título e data de acesso antes de entregar. -->

[1] GRIMANTICHEAT. *GrimAC* (anticheat para Minecraft). Repositório oficial e documentação no GitHub. Origem dos dados de violações e sessões.

[2] PAPERMC. *Paper* (servidor Minecraft). Documentação disponível em papermc.io.

[3] AUTHME. *AuthMe Reloaded* (plugin de autenticação). Repositório oficial no GitHub.

[4] GEYSERMC. *Geyser* e *Floodgate* (suporte a jogadores Bedrock). Documentação disponível em geysermc.org.

[5] VMWARE/BROADCOM. *Spring Boot* e *Spring JDBC (JdbcTemplate)*. Documentação disponível em spring.io.

[6] ORACLE. *MySQL* Reference Manual. Documentação disponível em dev.mysql.com.

[7] MICROSOFT. *.NET* e *Windows Forms*. Documentação disponível em learn.microsoft.com; LIVECHARTS. *LiveCharts2*; e *SkiaSharp* (bibliotecas de gráficos), repositórios oficiais no GitHub.

[8] COMO lidar com trapaceiros de jogos online: práticas recomendadas. *LinkedIn*. Disponível em: https://pt.linkedin.com/advice/0/what-best-practices-dealing-online-game-cheaters?lang=pt. Acesso em: 20 set. 2026. *(Usada apenas como contexto da justificativa, não como evidência dos resultados.)*

---

*Código-fonte: backend (Java/Spring Boot), importador de dados e dashboard (C#/WinForms) neste repositório.*
