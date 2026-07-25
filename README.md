# Loggin

Orquestrador de observabilidade composto por uma API ASP.NET Core e um cliente desktop moderno para Windows.

> O coletor em Go fica no projeto [Artses/Loggin_Collector](https://github.com/Artses/Loggin_Collector).

## Funcionalidades

- Autenticação e cadastro com JWT.
- CRUD de coletores e fontes de log.
- Coleta e pesquisa de linhas de log.
- Painel com indicadores e gráfico de atividade atualizado por polling.
- Reconexão automática com backoff exponencial, timeout e intervalo configuráveis.
- Tema claro, escuro ou do Windows, com paletas e cores personalizadas aplicadas sem reiniciar.
- Configurações persistidas em `%LOCALAPPDATA%\Artses\Loggin`.
- JWT armazenado no Gerenciador de Credenciais do Windows.
- Minimização, fechamento e menu de contexto na bandeja do sistema.
- Inicialização opcional com o Windows pelo registro do usuário atual.
- Publicações portáteis e instalador `win-x64`.

## Tecnologias

| Camada | Tecnologia |
|---|---|
| API | ASP.NET Core 8, Entity Framework Core, PostgreSQL, JWT, BCrypt |
| Desktop | WPF em .NET 8, MVVM leve, `HttpClient`, Windows Credential Manager |
| Gráfico | Controle WPF vetorial próprio (`DrawingContext`), sem dependência externa |
| Testes | xUnit e handler HTTP simulado |
| Instalador | Inno Setup 6 |
| CI/CD | GitHub Actions e GitHub CLI |

## Estrutura

```text
Controllers/                 Endpoints REST da API
DTOs/, Models/               Contratos e entidades da API
Services/, Repositories/     Regras e persistência da API
Loggin.Desktop/
  Controls/                  Gráfico WPF reutilizável
  Models/                    Contratos do cliente
  Services/                  API, tema, credenciais, configurações e Windows
  ViewModels/                Estado, comandos e fluxos assíncronos
Loggin.Desktop.Tests/        Testes dos contratos e falhas HTTP
installer/Loggin.iss         Definição do instalador
build/                       Scripts de ícone, release e notas
.github/workflows/           Pipeline de release
```

## Backend

### Requisitos

- .NET 8 SDK
- PostgreSQL (ou Docker Desktop)
- `JWT_KEY` com valor seguro, fora do controle de versão

Copie `env-example` para `.env`, ajuste as variáveis e suba o banco:

```powershell
docker compose up -d
dotnet run --project Api_Loggin.csproj
```

Em desenvolvimento, o Swagger fica em `http(s)://localhost:<porta>/swagger`.

### Endpoints consumidos

| Método | Rota | Função |
|---|---|---|
| POST | `/api/Auth/register` | Criar conta |
| POST | `/api/Auth/login` | Autenticar |
| GET | `/api/Auth/hi` | Validar sessão |
| GET/POST/PUT/DELETE | `/api/Collector` | CRUD de coletores |
| GET | `/api/Collector/{id}` | Consultar coletor |
| GET/POST/PUT/DELETE | `/api/Log` | CRUD de fontes |
| GET | `/api/Log/{id}` | Consultar fonte |
| POST | `/api/Log/GetLogs` | Buscar linhas no coletor |

Os contratos do backend não foram alterados. A API ainda não oferece WebSocket ou Server-Sent Events; por isso o cliente implementa tempo real por polling assíncrono. O intervalo padrão é 5 segundos e pode ser alterado em Configurações.

## Executar o desktop

```powershell
dotnet run --project Loggin.Desktop/Loggin.Desktop.csproj
```

Na primeira execução:

1. Informe a URL base da API na própria tela de login, incluindo `http://` ou `https://`.
2. Entre ou crie uma conta.
3. Em **Configurações**, ajuste timeout, frequência, tema e opções da bandeja.
4. Salve. Não é necessário reiniciar.

Os logs estruturados do cliente ficam em `%LOCALAPPDATA%\Artses\Loggin\logs`.

## Testes

```powershell
dotnet test Loggin.Desktop.Tests/Loggin.Desktop.Tests.csproj --configuration Release
```

Os testes verificam validação de URL/tema, contrato de login, autenticação Bearer, desserialização e propagação de erros do backend.

## Build manual

Publicação direta:

```powershell
dotnet publish Loggin.Desktop/Loggin.Desktop.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:Version=0.0.1 -o artifacts/publish
```

Build completo (API, testes, portátil e instalador):

```powershell
powershell -ExecutionPolicy Bypass -File build/Build-Release.ps1 -Version 0.0.1
```

O script exige Inno Setup 6 para criar o instalador. Use `-SkipInstaller` quando quiser gerar apenas a versão portátil. Saídas:

- `artifacts/Loggin-v0.0.1-win-x64-portable.zip`
- `artifacts/Loggin-Setup-v0.0.1-win-x64.exe`

O instalador usa um `AppId` fixo, portanto versões futuras atualizam a instalação existente. Ele registra desinstalação, instala em `%LOCALAPPDATA%\Programs`/diretório apropriado ao usuário e oferece atalho na área de trabalho.

## Versão e releases

O workflow `release.yml` roda em cada `push` para `main`:

1. Faz checkout com histórico e tags completos.
2. Encontra a maior tag `v0.0.N` e calcula `N + 1`.
3. Restaura, compila a API, executa testes e interrompe em caso de falha.
4. Injeta a versão nos metadados do desktop.
5. Publica a versão autocontida `win-x64`, compacta o portátil e compila o instalador.
6. Gera notas, cria uma tag anotada e publica a GitHub Release com os dois artefatos.

O workflow só observa pushes da branch `main`; o push da nova tag não dispara outra execução. A concorrência é serializada para evitar duas releases com o mesmo número. As únicas permissões concedidas são leitura de pull requests e escrita de conteúdo necessária para tags/releases.

### Notas e contribuidores

`build/Generate-ReleaseNotes.ps1` lista os commits desde a tag anterior, com links que preservam referências a issues e PRs. Os créditos combinam autores e trailers `Co-authored-by`, normalizam por e-mail quando possível e removem contas de automação/bots conhecidas. O autor principal é sempre **Artses**.

## Bandeja e inicialização do Windows

- Minimizar ou fechar pode esconder a janela, conforme a preferência salva.
- Duplo clique restaura a janela.
- O menu da bandeja abre o app, abre Configurações ou encerra o processo.
- A opção “Iniciar com o Windows” gerencia apenas `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`; não exige administrador.
- A versão portátil também pode ativar a inicialização, mas mover ou excluir a pasta depois disso invalida o caminho registrado. Desative a opção antes de mover o executável.

## Ícone e metadados

O fonte vetorial está em `Loggin.Desktop/Assets/loggin-mark.svg`. Para recriar o `.ico` multi-resolução:

```powershell
powershell -ExecutionPolicy Bypass -File build/Create-Icon.ps1
```

O ícone é usado pelo executável, janela, barra de tarefas, bandeja, instalador e atalhos. Nome, versão, descrição e autor ficam em `Loggin.Desktop.csproj` e recebem a versão da pipeline.

## Limitações conhecidas

- O “tempo real” depende de polling porque o backend não expõe transporte push.
- A disponibilidade de uma fonte também depende do coletor Go e do caminho configurado.
- Certificados HTTPS locais precisam ser confiáveis no Windows; a aplicação não desativa validação TLS.
- O tema “Sistema” é lido ao aplicar/salvar configurações; mudanças do Windows durante a sessão exigem reaplicar a prévia.
- Não há licença no repositório neste momento; escolha e adicione uma antes de distribuição pública.
