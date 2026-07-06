# API Loggin - Orquestrador de Observabilidade e Auditoria

> [!IMPORTANT]
> Este repositório contém a **API principal desenvolvida em C#**. 
> Para acessar o coletor de logs desenvolvido como biblioteca em **Go**, acesse:
> 👉 **[Loggin Collector (Go)](https://github.com/Artses/Loggin_Collector)**

---

## 📌 Sobre o Projeto

Este projeto faz parte de uma **solução de observabilidade de ponta a ponta** desenvolvida para auditoria, contextualização e gerenciamento centralizado de logs em aplicações distribuídas. A solução completa divide-se em duas partes principais:

1. **Coletor e Contextualizador (Biblioteca em Go)**: Integrado diretamente nas aplicações distribuídas por meio do framework Gin. Ele atua em tempo de execução coletando logs e injetando contexto relevante a cada evento.
2. **API Orquestradora (API em C# - Este Repositório)**: Desenvolvida em **ASP.NET Core**, é o cérebro da solução. Ela é responsável por orquestrar todo o processo de coleta, gerenciar os agentes coletores, persistir dados estruturados e definir o ciclo de vida dos dados de logs (incluindo tratamento, interpretação e disposição).

---

## 🚀 Funcionalidades da API C#

- **Gerenciamento de Coletores (CRUD)**: Cadastro e administração dos coletores de logs ativos (`Url`, `Path`, `Name`).
- **Autenticação & Autorização**: Proteção de endpoints utilizando **JWT (JSON Web Tokens)** e criptografia segura de senhas via **BCrypt**.
- **Migrações Automáticas**: Atualização e criação automática do esquema do banco de dados PostgreSQL na inicialização da aplicação (sem necessidade de aplicar migrações manuais em desenvolvimento).
- **Documentação Interativa**: Swagger UI pré-configurado e protegido com autenticação Bearer para testes fáceis dos endpoints diretamente pelo navegador.

---

## 🛠️ Tecnologias Utilizadas

- **Linguagem / Framework**: C# .NET 8.0 / ASP.NET Core
- **Banco de Dados**: PostgreSQL (via `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **ORM / Persistência**: Entity Framework Core (EF Core)
- **Segurança**: JWT Bearer Authentication & BCrypt.Net-Next
- **Documentação**: Swagger / OpenAPI (via Swashbuckle)

---

## 🗂️ Estrutura do Projeto

A API C# está organizada seguindo práticas recomendadas de separação de responsabilidades e injeção de dependências:

- **[Controllers](file:///e:/Loggin/Api_Loggin/Controllers)**: Exposição dos endpoints HTTP (`AuthController`, `CollectorController`).
- **[Services](file:///e:/Loggin/Api_Loggin/Services)**: Lógica de negócios da aplicação (`AuthServices`, `CollectorService`).
- **[Repositories](file:///e:/Loggin/Api_Loggin/Repositories)**: Abstração de acesso ao banco de dados PostgreSQL (`AuthRepository`, `CollectorRepository`).
- **[Models](file:///e:/Loggin/Api_Loggin/Models)**: Definição das entidades do banco de dados (`User`, `Collector`).
- **[DTOs](file:///e:/Loggin/Api_Loggin/DTOs)**: Objetos de transferência de dados para requisições e respostas.
- **[Data](file:///e:/Loggin/Api_Loggin/Data)**: Contexto de banco de dados do Entity Framework (`AppDbContext`).

---

## 🔑 Configuração e Execução

### Pré-requisitos
- **.NET 8.0 SDK** instalado.
- Servidor **PostgreSQL** em execução.

### 1. Configurar Conexões e Segredos
Ajuste as credenciais de banco de dados (`ConnectionStrings`) e a chave secreta do JWT no arquivo [appsettings.json](file:///e:/Loggin/Api_Loggin/appsettings.json) ou configure variáveis de ambiente correspondentes:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-jwt-de-pelo-menos-32-caracteres",
    "Issuer": "ProductsApi",
    "Audience": "ProductsApiUsers",
    "ExpiresInMinutes": 60
  }
}
```

### 2. Rodar a Aplicação
Execute o comando a seguir no terminal a partir do diretório raiz:

```powershell
dotnet run
```

As migrações pendentes do banco de dados serão aplicadas automaticamente no momento da inicialização.

### 3. Acessar o Swagger
Com a aplicação em execução no ambiente de desenvolvimento, acesse o painel interativo do Swagger pelo navegador:
👉 `http://localhost:<porta>/swagger/index.html` (verifique a porta gerada no console).

---

## 📌 Endpoints Principais

### Autenticação (`api/Auth`)
- `POST /api/Auth/register` - Cadastro de novos usuários.
- `POST /api/Auth/login` - Autenticação de usuário e retorno do token JWT.
- `GET /api/Auth/hi` (Protegido por token) - Validação de token com permissão `User`.

### Gerenciamento de Coletores (`api/Collector` - Todos requerem Token JWT)
- `POST /api/Collector` - Registra um novo coletor.
- `GET /api/Collector` - Lista todos os coletores registrados.
- `GET /api/Collector/{id}` - Obtém informações de um coletor específico.
- `PUT /api/Collector/{id}` - Atualiza as configurações de um coletor.
- `DELETE /api/Collector/{id}` - Remove um coletor do monitoramento.
