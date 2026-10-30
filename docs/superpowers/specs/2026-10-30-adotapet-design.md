# AdotaPet — Design

> Data: 2026-10-30 · Autor: Gustavo Piccoli · Status: rascunho para aprovação

## 1. Intenção e porquê

O Gustavo é desenvolvedor Full Stack (.NET, Angular, SQL Server). Todo o seu trabalho profissional fica em repositórios privados e depende de infraestrutura interna, por isso o GitHub público dele não mostra nada a um recrutador. O **AdotaPet** é o primeiro projeto de portfólio e existe para ser **visto e avaliado por recrutadores**: um site completo e bem acabado, feito na stack que ele domina, com qualidade de código, arquitetura e testes que ele consiga defender numa entrevista.

O AdotaPet é um site de **adoção de animais**: ONGs publicam pets disponíveis e adotantes enviam pedidos de adoção, que a ONG aprova ou recusa.

O projeto precisa começar pequeno, com uma primeira versão (v1) construída em algumas horas, e ter **grande potencial de expansão** (ver `docs/BACKLOG.md`).

## 2. Objetivos, não-objetivos e anti-objetivos

**Objetivos da v1**
- Fluxo completo de ponta a ponta: a ONG cadastra o pet, o adotante pede a adoção e a ONG aprova; depois disso o pet fica como adotado.
- Dois tipos de conta com login real (ONG e Adotante) e autorização por papel e por dono do dado.
- Repositório que roda com um comando (`docker compose up`) em qualquer máquina com Docker.
- README de vitrine: o que é, prints, como rodar, arquitetura e decisões.
- Testes automatizados (unitários e de integração) rodando em CI no GitHub Actions.

**Não-objetivos da v1** (vão para o backlog)
- Deploy público com link acessível.
- Galeria com várias fotos, chat, mapa, notificações por e-mail, refresh token, recuperação de senha.
- Adotante avisar a ONG sobre um animal que precisa de lar.
- Exclusão de pets, cancelamento de pedido pelo adotante, favoritos.

**Anti-objetivos** (o que tornaria o projeto um fracasso mesmo funcionando)
- Qualquer código, nome, tabela ou conceito copiado da PROCERGS/SEFAZ. O código é escrito do zero; o que se reaproveita são técnicas e padrões gerais.
- `docker compose` ou instruções do README que não funcionam numa máquina limpa: é a primeira coisa que um recrutador tenta.
- Visual genérico ou "cara de template": o site precisa parecer um produto.
- Arquitetura cerimoniosa sem propósito (camadas vazias, abstrações sem uso).

## 3. Restrições

- **Stack:** API em ASP.NET Core (.NET 10, C#), front em Angular 21 (standalone), banco SQL Server.
- **Execução local apenas** na v1: `docker compose` com os serviços `sqlserver`, `api` e `web`.
- **Máquina de desenvolvimento** (Windows 11): já tem Git, .NET SDK 10.0.300, Node 24/npm 12 e Python 3.14. **Faltam o WSL2 e o Docker Desktop**, que precisam de admin e reinício e serão instalados pelo Gustavo (`wsl --install --no-distribution` e `winget install -e --id Docker.DockerDesktop` num PowerShell como administrador). Até o reinício, só dá para construir e testar o que não depende do banco. Angular CLI via `npx @angular/cli` (ou instalação global).
- **Ponto de partida:** a pasta contém apenas este documento. O plano começa do zero e cria o restante, inclusive o `docs/BACKLOG.md` (a partir da seção 10) e o `CLAUDE.md`.
- **Repositório:** pasta `C:\Users\Gustavo\Desktop\Portifolio\AdotaPet\`, git próprio, depois publicado como repositório público no GitHub pessoal do Gustavo. Criar o repositório remoto e fazer push só com a confirmação dele.
- **Idioma:** código, nomes de tabelas e commits em inglês; interface do site em português (pt-BR).

## 4. Abordagens consideradas

| Abordagem | Prós | Contras | Decisão |
|---|---|---|---|
| **1. Clean Architecture em projetos separados** (Domain, Application, Infrastructure, Api, mais testes) | Mostra arquitetura madura; é o padrão que o Gustavo domina e consegue defender; cresce sem bagunça | Mais estrutura inicial | **Escolhida pelo Gustavo** |
| 2. Vertical Slices com Minimal APIs num projeto só | v1 mais rápida, estilo enxuto | Padrão diferente do que ele usa; um recrutador pode ler como "simples demais" | Rejeitada |
| 3. Clean Architecture + CQRS/MediatR | Padrão popular | Cerimônia sem ganho neste tamanho; MediatR tem licença comercial | Rejeitada |

## 5. Funcionalidades da v1

### Atores
- **Visitante** (sem login): navega pela vitrine e pela página de cada pet.
- **Adotante** (logado): tudo o que o visitante faz, mais enviar pedido de adoção e acompanhar os próprios pedidos.
- **ONG** (logada): cadastra e edita os próprios pets e aprova ou recusa os pedidos recebidos.

### Fluxos
1. **Cadastro e login.** Telas separadas de cadastro de ONG e de Adotante. Login único com e-mail e senha; o papel vem da conta.
2. **Vitrine.** Lista paginada no servidor (12 por página) apenas de pets *Disponíveis*, com filtros por espécie, porte, sexo, faixa de idade (o campo `AgeGroup`: filhote, adulto, idoso), UF e cidade. O card mostra foto, nome, espécie/porte/idade e a cidade da ONG.
3. **Página do pet.** Foto, todos os dados, descrição e a ONG responsável (nome, cidade, contato). Botão "Quero adotar":
   - visitante: leva ao login e volta para o pet depois de entrar;
   - adotante: abre o envio com campo de mensagem ("por que quero adotar"); se já existe pedido pendente dele para esse pet, mostra "Pedido em análise";
   - ONG: o botão não aparece.
4. **Meus pedidos (Adotante).** Lista dos pedidos com pet, ONG, data e situação (*Pendente*, *Aprovado*, *Recusado*).
5. **Meus pets (ONG).** Lista dos próprios pets (disponíveis e adotados), com cadastro e edição, incluindo o upload da foto.
6. **Pedidos recebidos (ONG).** Lista dos pedidos dos seus pets com os dados de contato do adotante e a mensagem; ações *Aprovar* e *Recusar* nos pendentes. Filtro por situação.

## 6. Modelo de dados

Nomes no código e no banco em inglês. Na interface, *Shelter* aparece como "ONG".

**Shelter** (ONG)
- `Id`, `UserId` (conta), `Name`, `Cnpj` (14 dígitos, dígitos verificadores validados, único), `City`, `State` (UF, 2 letras), `Phone`, `Email`, `Description`, `CreatedAt`.

**Adopter** (Adotante)
- `Id`, `UserId`, `Name`, `Phone`, `City`, `State`, `CreatedAt`. O e-mail vem da conta. Sem CPF, por decisão do Gustavo: é dado sensível e não é necessário.

**Pet**
- `Id`, `ShelterId`, `Name`, `Species` (Dog, Cat, Other), `Sex` (Male, Female), `Size` (Small, Medium, Large), `AgeGroup` (Puppy, Adult, Senior), `IsNeutered`, `IsVaccinated`, `Description`, `PhotoPath` (nulo até o upload), `Status` (Available, Adopted), `CreatedAt`, `UpdatedAt`, `RowVersion` (concorrência otimista).
- A cidade e a UF do pet são as da ONG.

**AdoptionRequest** (Pedido de adoção)
- `Id`, `PetId`, `AdopterId`, `Message`, `Status` (Pending, Approved, Rejected), `CreatedAt`, `DecidedAt`.

**Conta de usuário:** ASP.NET Core Identity (tabelas próprias), com papéis `Shelter` e `Adopter`. O Domain não depende do Identity: `Shelter` e `Adopter` guardam apenas o `UserId`.

## 7. Regras de negócio

1. Só a ONG dona edita um pet e vê ou decide os pedidos desse pet. Acesso a dado de outra ONG retorna 404, sem revelar que o recurso existe.
2. Só adotantes enviam pedidos, e só para pets *Disponíveis*.
3. **Um pedido pendente por adotante e pet.** Garantido no domínio e no banco por índice único filtrado (`UNIQUE (AdopterId, PetId) WHERE Status = Pending`). Depois de uma recusa, o adotante pode pedir de novo.
4. Só pedidos *Pendentes* podem ser aprovados ou recusados.
5. **Aprovar um pedido**, numa única transação: o pedido vira *Aprovado*, o pet vira *Adotado* e todos os outros pedidos pendentes do pet viram *Recusados*. Duas aprovações simultâneas para o mesmo pet não podem ambas vencer: a mudança de status do pet usa concorrência otimista (`RowVersion`), e a perdedora recebe 409 Conflict.
6. Pet *Adotado* sai da vitrine; continua visível na lista da ONG e acessível na página dele, sem o botão de adoção.
7. A ONG pode editar os dados de um pet, mas a situação (*Disponível*/*Adotado*) só muda pela aprovação de um pedido. Na v1, *Adotado* é definitivo; reabrir um pet cuja adoção não deu certo fica no backlog.
8. Entidades se validam no construtor e nos métodos de mudança de estado; violações lançam exceções de domínio, que a API traduz em `ProblemDetails` (400, 404 ou 409).

## 8. Arquitetura

### Backend
Solução `AdotaPet.sln` em `src/` e `tests/`:

```
src/
  AdotaPet.Domain/          entidades, enums, exceções de domínio, interfaces de repositório. Sem pacotes NuGet.
  AdotaPet.Application/     casos de uso (interface + implementação), DTOs (sealed records), interfaces de serviços técnicos
  AdotaPet.Infrastructure/  EF Core (DbContext, configurações, migrations, repositórios), Identity, emissão de JWT, armazenamento de fotos
  AdotaPet.Api/             controllers, autenticação/autorização, tratamento de exceções → ProblemDetails, OpenAPI + Scalar, composição do DI
tests/
  AdotaPet.Domain.Tests/        unitários das regras das entidades
  AdotaPet.Application.Tests/   unitários dos casos de uso com fakes
  AdotaPet.Api.IntegrationTests/ API de ponta a ponta (WebApplicationFactory + Testcontainers com SQL Server)
```

- Dependências só para dentro (`Domain ← Application ← Infrastructure ← Api`), garantidas pelas referências entre projetos.
- **Organização por funcionalidade** dentro de cada projeto (`Pets/`, `AdoptionRequests/`, `Shelters/`, `Adopters/`, `Accounts/`), com o aspecto técnico como subpasta.
- **Casos de uso como interface + implementação**, sem mediator, registrados explicitamente no DI. Erros sinalizados por exceções de domínio.
- **Dados:** EF Core 10 com migrations; migrations aplicadas na subida da API em ambiente de desenvolvimento/compose.
- **Autenticação:** ASP.NET Core Identity para usuários e senhas; o login emite um JWT (60 min, sem refresh token na v1) com o papel e o id do perfil como claims. O cadastro cria o usuário do Identity e o perfil (`Shelter` ou `Adopter`) numa única transação, sem deixar usuário órfão. Autorização por política de papel mais checagem de dono nos casos de uso.
- **Fotos:** interface `IPhotoStorage` (Application) com implementação em disco local (Infrastructure), numa pasta montada como volume Docker. Aceita JPEG, PNG e WebP até 5 MB, validando o conteúdo real do arquivo (assinatura) e não só a extensão. Nome do arquivo gerado pelo servidor (GUID). A API serve a foto por um endpoint público.
- **Logs:** Serilog no console. **Documentação da API:** OpenAPI + Scalar em desenvolvimento.

### Endpoints (rascunho; o formato fica a critério do builder)
| Método | Rota | Quem |
|---|---|---|
| POST | `/api/auth/register/shelter`, `/api/auth/register/adopter`, `/api/auth/login` | público |
| GET | `/api/pets` (filtros + paginação), `/api/pets/{id}`, `/api/pets/{id}/photo` | público |
| GET/POST/PUT | `/api/shelter/pets`, `/api/shelter/pets/{id}`; PUT `/api/shelter/pets/{id}/photo` | ONG |
| GET | `/api/shelter/adoption-requests?status=` | ONG |
| POST | `/api/shelter/adoption-requests/{id}/approve`, `/reject` | ONG |
| POST | `/api/pets/{id}/adoption-requests` | Adotante |
| GET | `/api/adopter/adoption-requests` | Adotante |
| GET | `/api/me` | logado |

### Frontend
Angular 21 com componentes standalone, **signals** para estado, `inject()`, guards e interceptors funcionais, Tailwind CSS. A identidade visual é criada com a skill `frontend-design`: acolhedora e própria, sem cara de template. Layout responsivo (celular e desktop).

Rotas: `/` (vitrine), `/pets/:id`, `/entrar`, `/cadastro/ong`, `/cadastro/adotante`, `/meus-pedidos` (Adotante), `/ong/pets`, `/ong/pets/novo`, `/ong/pets/:id/editar`, `/ong/pedidos` (ONG).

- Interceptor que injeta o JWT e trata 401 (desloga e manda para o login).
- Guards por papel.
- Token guardado no `localStorage` (débito conhecido, registrado no backlog: migrar para cookie httpOnly).
- Feedback de erro padronizado (toast) a partir dos `ProblemDetails` da API.

### Dados de demonstração
Seed executado só no ambiente `Development` (o compose sobe a API nesse ambiente):
- 2 ONGs com CNPJs fictícios gerados com dígitos verificadores válidos;
- cerca de 12 pets, 1 ou 2 já adotados, com fotos baixadas durante a implementação do Unsplash ou do Pexels (licenças livres) e versionadas no repositório, com os créditos no README;
- 2 adotantes;
- pedidos de exemplo em cada situação (pendente, aprovado, recusado), para o painel da ONG e o "Meus pedidos" já terem conteúdo.

As credenciais de demonstração ficam no README. Isso deixa o site "vivo" logo ao subir e permite tirar os prints.

## 9. Infraestrutura, qualidade e documentação

- **`docker-compose.yml`:** `sqlserver` (imagem oficial `mcr.microsoft.com/mssql/server`), `api` (Dockerfile multi-stage) e `web` (build do Angular servido por nginx, com proxy de `/api` para a API). Volumes para os dados do banco e para as fotos. Um `.env.example` com as variáveis (senha do SA, chave JWT).
- **Testes:** xUnit no backend (unitários de Domain e Application e integração com Testcontainers). No front, testes de serviços, guards e componentes principais com o runner padrão do Angular 21.
- **CI (GitHub Actions):** a cada push e PR, build e testes do backend (incluindo integração; o runner Ubuntu tem Docker) e build, lint (angular-eslint, com as regras padrão) e testes do front.
- **`README.md`:** o que é o projeto, prints/GIF, como rodar, credenciais de demonstração, diagrama da arquitetura, decisões técnicas, stack e link para o backlog.
- **`docs/BACKLOG.md`:** lista viva de débitos técnicos, funcionalidades futuras e melhorias.
- **`CLAUDE.md`:** guia do projeto para o Claude Code (comandos, arquitetura, convenções) e aponta para `docs/BACKLOG.md` como fonte única do backlog. O backlog **não** é duplicado no CLAUDE.md.

## 10. Backlog inicial (sementes do `docs/BACKLOG.md`)

- Deploy público (Azure App Service + Azure SQL, armazenamento de fotos em nuvem).
- Adotante avisa uma ONG sobre um animal que precisa de lar.
- Galeria com várias fotos por pet.
- Cancelamento de pedido pelo adotante; favoritos.
- Inativar/excluir pet; reabrir um pet adotado (adoção que não deu certo).
- Notificações por e-mail (pedido recebido, pedido decidido).
- Refresh token e recuperação de senha; migrar o token do `localStorage` para cookie httpOnly.
- Cidades e UFs a partir da API do IBGE.
- Painel da ONG com números (pedidos por mês, adoções).
- Testes E2E com Playwright.

## 11. Decisões tomadas pelo Gustavo
- Tema: site de adoção de animais, sem relação com a PROCERGS.
- v1 só com repositório e execução local via Docker; deploy vai para o backlog.
- Backlog em `docs/BACKLOG.md`, citado no CLAUDE.md.
- Login de verdade, com dois tipos de conta (ONG e Adotante).
- Adotante na v1: apenas pedir adoção e acompanhar os pedidos; limite de um pedido pendente por pet.
- Dados das entidades como propostos (sem CPF, cidade do pet = cidade da ONG, uma foto).
- Arquitetura: Clean Architecture em projetos separados.
- Instalar WSL2 + Docker Desktop, com o reinício feito depois pelo Gustavo.
- Nome do projeto e do repositório: **AdotaPet**.
- Commits **sem** coautoria da IA (`Co-Authored-By` ou qualquer outra forma), nem em descrições de PR.
- `CLAUDE.md` e `docs/superpowers/` (design e plano) **são versionados** e ficam públicos no repositório.

## 12. Decisões do Claude (propostas, sujeitas à revisão do Gustavo)
- EF Core + migrations; ASP.NET Core Identity + JWT; xUnit + Testcontainers; Angular com signals + Tailwind; Serilog; OpenAPI + Scalar.
- Recusa automática dos outros pedidos pendentes quando um pedido é aprovado; concorrência otimista na aprovação.
- Novo pedido permitido depois de uma recusa.
- Acesso a dado de outra ONG retorna 404.
- Paginação de 12 itens; JWT de 60 min sem refresh na v1.
- Fotos até 5 MB (JPEG/PNG/WebP) em disco local atrás de `IPhotoStorage`.
- Seed de demonstração (só em `Development`) com imagens do Unsplash/Pexels, CNPJs fictícios válidos, 2 adotantes e pedidos de exemplo em cada situação.
- Filtro de idade pelo `AgeGroup`; *Adotado* é definitivo na v1.
- Cadastro cria usuário e perfil numa única transação.
- Lint do front com angular-eslint.

## 13. O que fica a critério do builder
- Formato exato das rotas, dos DTOs e dos códigos de resposta, dentro das regras acima.
- Nomes de classes e organização interna das subpastas.
- Escolha da biblioteca de toast e dos componentes de UI, desde que siga a identidade criada com o `frontend-design`.
- Detalhes do Dockerfile, do nginx e do workflow de CI.
- Ordem das tarefas no plano, desde que o que depende do Docker venha depois do reinício do PC.
