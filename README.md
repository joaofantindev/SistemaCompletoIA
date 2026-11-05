# CompleteSystem

Sistema de estudos em **ASP.NET Core MVC (.NET 10)** para praticar, em um único monólito bem dividido, os conceitos que costumam aparecer separados:

- **DTOs** — contratos de entrada e saída que não vazam entidades nem dados sensíveis;
- **Encapsulamento e SOLID** — controllers dependem de interfaces, services concentram regras de negócio e o `DbContext` concentra persistência;
- **ASP.NET Core Identity** — autenticação por cookie, hash de senha, bloqueio por tentativas e autorização por perfis;
- **Testes de todos os tipos** — unitários de service, testes de segurança por reflexão e testes de integração HTTP.

A aplicação gerencia **usuários, produtos, categorias, regras de desconto, compras e logs de acesso**, com permissões diferentes para cada perfil.

---

## 📋 O que você já tinha feito × o que foi implementado nesta etapa

Esta seção separa o que já existia no projeto do que foi construído na continuação solicitada em `AGENT_LOGIC.md`, para deixar claro o histórico de autoria e evolução.

### ✅ O que você já tinha feito

- Estrutura do projeto em **ASP.NET Core MVC (.NET 10)** com **EF Core + SQLite**.
- **CRUD de usuários completo**: entidade `Usuario`, DTOs de criação/edição/resposta, `UsuarioMapper`, `IUsuarioService`, `UsuarioService`, `UsuarioController` e views de Index, Create, Edit, Details e Delete.
- **Hash de senha** com `IPasswordHasher<Usuario>`/`PasswordHasher<Usuario>` e coluna `SenhaHash`.
- **Produto parcialmente iniciado**: entidade `Produto`, DTOs, mapper, `IProdutoService`/`ProdutoService` com o CRUD básico e `ProdutoController` apenas com a action `Index`.
- **Migrations iniciais** `InitialCreate` e `produtoCriado`, com aplicação automática no startup via `Database.Migrate()`.
- **Injeção de dependência** dos services de usuário e produto.
- Views de usuário, layout básico e materiais de estudo (`fluxo.md`, `studyWhileCode.md`, `guia/`).
- Banco `app.db` com o usuário `joao fantin` (`j@gmail.com`).

Naquele momento ainda não existiam: autenticação/login, perfis de acesso, categorias, regras de desconto, compras, logs de acesso, views de produto, vínculo de produto com proprietário/categoria e testes automatizados.

### 🔧 O que foi implementado nesta etapa

- **Conclusão da lógica de Produtos**:
  - vínculo obrigatório com categoria e vínculo com o proprietário autenticado;
  - validação de categoria existente/ativa, preço e nome duplicado por categoria;
  - controller completo (Index, Details, Create, Edit, Delete) e todas as views;
  - listagem de produtos por categoria.
- **Integração completa com ASP.NET Core Identity**:
  - `Usuario` passou a herdar de `IdentityUser<int>`;
  - `AppDbContext` passou a herdar de `IdentityDbContext<Usuario, IdentityRole<int>, int>`;
  - login/logout por cookie, tela de acesso negado, bloqueio após tentativas inválidas e seed do administrador inicial;
  - `SenhaHash` renomeado para o `PasswordHash` do Identity, preservando os hashes existentes.
- **Perfis e autorização**:
  - constantes `Admin`, `Manager`, `Owner` e `Client`;
  - permissões aplicadas com `[Authorize(Roles = ...)]` em cada controller/action;
  - Admin gerencia usuários, perfis e logs; Manager gerencia categorias/regras; Owner cadastra produtos; Client compra;
  - proteção para não remover nem rebaixar o último administrador ativo.
- **Novos domínios**:
  - `Categoria` e `RegraCategoria` (regras de desconto por categoria);
  - `Compra` com snapshot do produto e cálculo do maior desconto ativo;
  - `RegistroAcesso` para auditar login, falhas, requisições e acessos negados.
- **Novos services, controllers e views** para categorias, regras, compras e logs, todos atrás de interfaces.
- **Middleware de log de requisições**, usando um escopo de DI próprio para não interferir no `DbContext` da requisição.
- **Segurança adicional**: CSRF em todos os POSTs, usuário inativo sem login, DTOs sem senha/hash, validação de redirecionamento local e controle de acesso por perfil.
- **Padronização do código**:
  - namespaces unificados em `CompleteSystem.*`;
  - variáveis, métodos e parâmetros renomeados para nomes completos e descritivos;
  - encapsulamento e responsabilidades mantidos (Controller → Service → DbContext).
- **Testes automatizados**: projeto `CompleteSystem.Tests` com **62 testes** unitários, de segurança e de integração HTTP.
- **Migration `IdentityPerfisCategoriasComprasLogs`** criada e aplicada no banco real, com preservação dos dados existentes.
- **README reescrito** para documentar o sistema completo, os perfis, os fluxos, a segurança e os testes.
- Ajuste no `CompleteSystem.csproj` para impedir que o projeto web compile os arquivos do projeto de testes.

### Resumo por área

| Área | Já existia | Implementado nesta etapa |
| --- | --- | --- |
| Usuários | CRUD, DTOs, mapper, service e views | Identity, login/logout, perfis, seed e proteção do último admin |
| Produtos | Entidade, DTOs, service CRUD e controller só com `Index` | Regras completas, categoria/proprietário, controller e views |
| Categorias | Não existia | Entidade, service, controller, views e regras de remoção |
| Regras de desconto | Não existia | Entidade, service, controller, views e aplicação na compra |
| Compras | Não existia | Entidade, service, controller, views, snapshot e desconto |
| Logs | Não existia | Entidade, service, middleware, controller e views |
| Segurança | Hash de senha e CSRF nas actions existentes | Roles, autorização, lockout, usuário inativo e proteção do admin |
| Testes | Não existia | 62 testes unitários, de segurança e de integração |

---

## 👥 Perfis de acesso

| Perfil | Constante | Pode fazer |
| --- | --- | --- |
| **Admin** | `PerfisAcesso.Administrador` | Criar usuários e administradores, alterar o perfil de qualquer usuário, remover usuários e gerenciar os logs de acesso/requisições. Também acessa categorias, produtos e compras. |
| **Manager** | `PerfisAcesso.Gerente` | Criar, editar e remover categorias de produtos e regras de desconto das categorias. |
| **Owner** | `PerfisAcesso.Proprietario` | Cadastrar, editar e remover produtos, sempre vinculados a uma categoria. |
| **Client** | `PerfisAcesso.Cliente` | Visualizar produtos e realizar compras. |

As regras de autorização ficam explícitas nos controllers com `[Authorize(Roles = ...)]`, usando as constantes de [`Models/PerfisAcesso.cs`](Models/PerfisAcesso.cs). Um usuário sem o perfil exigido é redirecionado para `/Account/AccessDenied`.

> O sistema impede que o **último administrador ativo** seja removido ou rebaixado.

### Administrador inicial

Na primeira execução, [`Data/InicializadorBancoDados.cs`](Data/InicializadorBancoDados.cs) cria os quatro perfis e um administrador padrão:

```text
E-mail: admin@completesystem.com
Senha:  Admin@12345
```

Esses valores vêm de `appsettings.json`, na seção `AdministradorInicial`. **Troque a senha em qualquer ambiente real.**

---

## 🏗️ Arquitetura

```text
Requisição HTTP
      │
      ▼
┌─────────────────┐   recebe DTO, valida ModelState, escolhe a resposta
│   Controller    │   (não tem DbContext, não faz hash, não conhece regra de negócio)
└────────┬────────┘
         │ depende de interfaces (IUsuarioService, IProdutoService, ...)
         ▼
┌─────────────────┐   valida regras, normaliza dados, calcula descontos
│     Service     │   (não conhece View, IActionResult nem TempData)
└────────┬────────┘
         │ usa AppDbContext, UserManager e RoleManager
         ▼
┌─────────────────┐   consulta e grava com EF Core
│   DbContext     │
└────────┬────────┘
         ▼
      SQLite (app.db)
```

### Estrutura de pastas

```text
CompleteSystem/
├── Controllers/
│   ├── AccountController.cs            # login, logout e acesso negado
│   ├── HomeController.cs               # páginas institucionais / erro
│   ├── UsuarioController.cs            # Admin: usuários e perfis
│   ├── CategoriaController.cs          # Admin/Manager: categorias
│   ├── RegraCategoriaController.cs     # Admin/Manager: regras de desconto
│   ├── ProdutoController.cs            # Admin/Owner: produtos
│   ├── CompraController.cs             # Admin/Client: compras
│   └── RegistroAcessoController.cs     # Admin: logs de acesso
├── Models/
│   ├── Usuario.cs                      # IdentityUser<int>
│   ├── Produto.cs
│   ├── Categoria.cs
│   ├── RegraCategoria.cs
│   ├── Compra.cs
│   ├── RegistroAcesso.cs
│   ├── PerfisAcesso.cs                 # constantes Admin/Manager/Owner/Client
│   └── DTOs/                           # entrada, saída e mapeadores
├── Data/
│   ├── AppDbContext.cs                 # IdentityDbContext + mapeamento
│   ├── InicializadorBancoDados.cs      # migrations, perfis e admin inicial
│   └── Migrations/
├── Middleware/
│   └── RegistroRequisicaoMiddleware.cs # log automático de requisições
├── Services/
│   ├── I*Service.cs                    # contratos (fronteiras)
│   ├── *Service.cs                     # regras de negócio
│   └── RegraDeNegocioException.cs
├── Views/                              # Razor, organizadas por controller
├── CompleteSystem.Tests/               # testes unitários, segurança e integração
├── Program.cs                          # DI, Identity e pipeline HTTP
└── appsettings.json                    # connection string e admin inicial
```

---

## 🔄 Fluxos principais

### Login e sessão

1. `/Account/Login` recebe `LoginDTO` (e-mail e senha).
2. O controller busca o usuário no `UserManager`, recusa usuários inativos e usa `SignInManager.PasswordSignInAsync` com bloqueio após 5 tentativas inválidas.
3. Sucesso e falha são gravados em `RegistrosAcesso`.
4. O cookie de autenticação expira em 8 horas, com renovação por uso.

### Admin — usuários e perfis

- `UsuarioController` exige o perfil **Admin**.
- `UsuarioService` cria o usuário via `UserManager`, atribui o perfil escolhido e valida e-mail duplicado.
- A edição permite trocar o perfil, mas impede rebaixar o último administrador ativo.
- A remoção também impede excluir o último administrador ativo.

### Manager — categorias e regras

- Categorias têm nome, descrição e indicador de ativa.
- Regras pertencem a uma categoria e definem um percentual de desconto entre 0% e 100%.
- Uma categoria com produtos ou regras vinculados não pode ser removida.
- Apenas regras ativas são consideradas no cálculo da compra.

### Owner — produtos

- O cadastro exige nome, marca, preço e categoria.
- O proprietário é obtido do usuário autenticado, nunca do formulário.
- Não é permitido cadastrar dois produtos com o mesmo nome na mesma categoria.
- Produtos de categorias inativas não podem ser comprados.

### Client — compras

1. O cliente escolhe um produto e a quantidade.
2. `CompraService` valida produto, quantidade e status da categoria.
3. O sistema busca a **maior regra ativa** da categoria e aplica o desconto.
4. A compra guarda um snapshot de nome, valor unitário, desconto e total, para que o histórico não mude se o produto for editado ou removido.
5. O cliente vê apenas as próprias compras; o Admin vê todas.

### Logs de acesso e requisições

[`Middleware/RegistroRequisicaoMiddleware.cs`](Middleware/RegistroRequisicaoMiddleware.cs) grava método, caminho, status, IP, navegador e usuário de cada requisição. O middleware cria um escopo de DI próprio para não interferir no `DbContext` da requisição e ignora arquivos estáticos.

O Admin pode filtrar os registros por e-mail, tipo de ação e período, além de abrir detalhes e remover registros antigos. Os tipos registrados são:

- `LoginBemSucedido`
- `LoginFalho`
- `Requisicao`
- `AcessoNegado`

---

## 🔐 Decisões de segurança

| Medida | Onde aparece |
| --- | --- |
| Senhas apenas como hash do Identity | `UserManager` + coluna `PasswordHash` |
| E-mail único e normalizado | `AppDbContext` + configuração do Identity |
| Bloqueio temporário após falhas | `PasswordSignInAsync(lockoutOnFailure: true)` |
| Autorização por perfil | `[Authorize(Roles = ...)]` nos controllers |
| Proteção CSRF | `[ValidateAntiForgeryToken]` em todos os POSTs |
| Usuário inativo não autentica | `AccountController.Login` |
| DTOs de resposta sem senha/hash | `UsuarioResponseDTO` e demais DTOs |
| Último admin protegido | `UsuarioService` |
| Log de tentativas de login | `RegistroAcessoService` |
| Redirecionamento local validado | `Url.IsLocalUrl` no login |

---

## 🧪 Testes

O projeto de testes fica em [`CompleteSystem.Tests`](CompleteSystem.Tests) e usa **xUnit**, **SQLite in-memory** e **WebApplicationFactory<Program>**.

```bash
dotnet test CompleteSystem.Tests/CompleteSystem.Tests.csproj
```

Cobertura atual: **62 testes**, divididos em:

| Tipo | Arquivos | O que cobre |
| --- | --- | --- |
| Unitários de service | `Servicos/*Tests.cs` | Regras de usuário, produto, categoria, regra e compra |
| Segurança por reflexão | `Seguranca/AutorizacaoTests.cs` | Perfis exatos, `[Authorize]` por action e DTOs sem dados sensíveis |
| Integração HTTP | `Integracao/RotasEProtecaoTests.cs` | Login real, cookies, redirecionamentos, CSRF e acesso por perfil |

Os testes de integração usam um arquivo SQLite temporário e ambiente `Testing`, no qual o pipeline dispensa HTTPS e o handler de exceção de produção.

---

## 🚀 Como executar

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Opcional: `dotnet-ef` para criar novas migrations

### Passos

```bash
dotnet restore
dotnet run
```

A aplicação sobe em <http://localhost:5210> (perfil HTTP) ou <https://localhost:7238> (perfil HTTPS).

Na inicialização, `Program.cs` chama `InicializadorBancoDados`, que:

1. aplica as migrations pendentes;
2. cria os perfis `Admin`, `Manager`, `Owner` e `Client`;
3. cria o administrador inicial, se ainda não existir.

### Migration manual

```bash
dotnet ef database update
dotnet ef migrations add NomeDaMigration
```

O banco usado é o `app.db` na raiz do projeto. Faça backup antes de aplicar migrations em dados reais.

---

## 🧩 Principais decisões de projeto

| Decisão | Motivo |
| --- | --- |
| Um DTO por intenção (Create, Update, Response) | Cada operação tem contrato próprio e evita over-posting |
| Services atrás de interfaces | Inversão de dependência, testabilidade e fronteira clara |
| `Usuario : IdentityUser<int>` | Reaproveita hash, normalização, lockout e perfis do Identity |
| `IdentityDbContext<Usuario, IdentityRole<int>, int>` | Mantém o Identity no mesmo banco SQLite do domínio |
| Constantes em `PerfisAcesso` | Elimina strings soltas e erros de digitação nas roles |
| Snapshot na `Compra` | Histórico confiável mesmo após edição/remoção do produto |
| `RegraDeNegocioException` | Separa erro de negócio de erro técnico e de validação de formulário |
| `AsNoTracking()` nas leituras | Consultas de listagem/detalhe sem rastreamento desnecessário |
| Middleware com escopo próprio | Log não interfere no `DbContext` nem no resultado da requisição |

Regra de ouro:

> **Controller não sabe SQL. Service não sabe HTTP. Entidade não sabe validação de formulário.**

---

## 🛠️ Stack

- .NET 10 / ASP.NET Core MVC
- C# com nullable reference types habilitado
- Entity Framework Core 10
- SQLite
- ASP.NET Core Identity (cookie + roles)
- Razor Views + Bootstrap
- xUnit + WebApplicationFactory

---

## 📌 Próximos passos sugeridos

- [ ] Paginação e busca nas listagens
- [ ] Recuperação de senha e confirmação de e-mail
- [ ] Relatórios de vendas para o Admin
- [ ] Extrair os services para uma API separada
- [ ] Adicionar OpenAPI/Swagger
- [ ] Substituir SQLite por PostgreSQL em produção
- [ ] Configurar renovação e auditoria de logs em armazenamento dedicado

---

## 👤 Autor

Projeto pessoal de estudos em .NET, com foco em **entender o porquê** de cada camada — e não apenas fazer o CRUD funcionar.
> O SISTEMA VAI SER MELHORADO E USADO COMO BASE PARA OUTROS PROJETOS, MAIS INTELIGENTE DO QUE RECRIAR DO ZERO
