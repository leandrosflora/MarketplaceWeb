# Instruções do repositório

Use como referência arquitetural canônica o repositório [`leandrosflora/logistica-envios-demo-arch`](https://github.com/leandrosflora/logistica-envios-demo-arch).

Ao alterar integrações HTTP, contratos de entrada/saída ou documentação deste frontend, mantenha compatibilidade com os contratos do MarketplaceWeb.Bff documentados em `docs/contracts/logistica-envios-apis.openapi.yaml` no repositório de referência.

## Contexto do frontend

Este projeto é uma aplicação web server-side em ASP.NET Core 8 com Razor Pages. A interface usa Bootstrap, jQuery, validação unobtrusive e estilos próprios em `wwwroot/css/site.css`.

A base visual canônica do projeto está em:

- `Pages/Shared/_Layout.cshtml` para shell, header, busca global, navegação e footer.
- `wwwroot/css/site.css` para tokens, estilos globais e componentes visuais.
- `wwwroot/js/site.js` para comportamentos globais simples, como loading em submit e máscara de CEP.
- `Pages/**/*.cshtml` para padrões reais de página, cards, formulários, tabelas, alertas e estados.

## Regras de design system

Ao alterar ou criar telas, preserve e evolua o design system existente. Não crie uma nova linguagem visual paralela.

### Tokens

Use os tokens CSS existentes com prefixo `--marketplace-*` para cores, superfícies, bordas, sombras e raios.

Não use cores hardcoded quando já existir token equivalente em `wwwroot/css/site.css`.

Exemplos de tokens canônicos:

- `--marketplace-background`
- `--marketplace-surface`
- `--marketplace-surface-soft`
- `--marketplace-border`
- `--marketplace-border-strong`
- `--marketplace-primary`
- `--marketplace-primary-hover`
- `--marketplace-primary-soft`
- `--marketplace-secondary`
- `--marketplace-secondary-hover`
- `--marketplace-warning`
- `--marketplace-header`
- `--marketplace-text`
- `--marketplace-muted`
- `--marketplace-shadow`
- `--marketplace-shadow-sm`
- `--marketplace-radius`
- `--marketplace-radius-sm`

Se precisar de uma nova cor, sombra, raio ou espaçamento recorrente, crie primeiro um token semântico em `:root` e depois consuma esse token nas classes.

### Componentes visuais

Reutilize os padrões já existentes antes de criar novas classes:

- `marketplace-header` para cabeçalho.
- `marketplace-logo` para marca no header.
- `marketplace-card` para cards e blocos de conteúdo.
- `marketplace-panel` para painéis com borda, superfície e overflow controlado.
- `marketplace-hero` para blocos de destaque.
- `marketplace-eyebrow` para rótulos curtos de seção.
- `marketplace-list-item` para itens em listas verticais.
- `product-search-card` para cards clicáveis de produto.
- `shipping-result` para resultado de frete.
- `tracking-event` e `tracking-dot` para linha do tempo de tracking.

Use os componentes Bootstrap customizados pelo projeto para botões, formulários, tabelas, badges, alertas e paginação. Evite CSS local duplicando comportamento de `.btn`, `.form-control`, `.table`, `.badge`, `.alert` ou `.pagination`.

### Layout

Use `container py-4` como estrutura padrão de página.

Use grid Bootstrap (`row`, `col-*`, `g-*`) para composição responsiva.

Para páginas com destaque inicial, siga o padrão de `Pages/Index.cshtml` e `Pages/Search.cshtml`:

- `marketplace-card marketplace-hero`
- `marketplace-eyebrow`
- título com escala Bootstrap (`h1`, `h2`, `display-*` quando fizer sentido)
- texto de apoio curto

Para páginas transacionais, siga os padrões existentes:

- checkout: conteúdo principal à esquerda e resumo à direita;
- pedido: seções em `marketplace-card` com status em `badge`;
- produto: imagem/placeholder à esquerda e compra/frete à direita;
- busca/listagem: filtros ou cabeçalho, contagem de resultados e cards/tabela.

### Estados obrigatórios

Toda página nova ou alterada deve tratar, quando aplicável:

- estado vazio;
- estado de erro;
- mensagens de sucesso;
- mensagens de alerta/warning vindas do BFF;
- validação de formulário com `asp-validation-summary` ou mensagens por campo;
- loading de submit usando `data-loading-text` nos botões.

Para formulários, mantenha o padrão de `wwwroot/js/site.js`: botões de submit devem ter `data-loading-text` quando a ação puder demorar.

### Acessibilidade e semântica

Use HTML semântico nos arquivos `.cshtml`:

- `section` para blocos de conteúdo;
- `article` para cards de itens clicáveis ou repetidos;
- headings em ordem lógica;
- `role="alert"` em mensagens críticas quando necessário;
- labels com tag helpers `asp-for` em campos de formulário;
- `inputmode`, `maxlength`, `pattern` e `title` quando ajudarem na entrada de dados.

Não remova foco visível. O foco de `.btn`, `.form-control`, `.form-select` e `.form-check-input` já está padronizado em `site.css`.

### CSS

Mantenha estilos compartilhados em `wwwroot/css/site.css`.

Evite estilos inline em `.cshtml`, exceto casos realmente pontuais e justificados.

Ao criar uma classe nova:

1. Verifique se um padrão existente resolve.
2. Use prefixo `marketplace-` para estilos genéricos do design system.
3. Use prefixo específico da tela somente para estilos que não devem ser reaproveitados.
4. Consuma tokens `--marketplace-*` sempre que possível.

Não adicione uma biblioteca visual nova sem necessidade real. O padrão atual é Bootstrap customizado por CSS próprio.

### JavaScript

Mantenha `wwwroot/js/site.js` restrito a comportamentos globais simples e progressivos.

Não coloque regra de negócio no JavaScript do frontend. Regras transacionais continuam no BFF e nos serviços de domínio.

Novos comportamentos globais devem:

- funcionar sem quebrar renderização server-side;
- preservar validação HTML/Razor existente;
- usar atributos `data-*` quando possível;
- ser seguros para páginas que não possuem o elemento alvo.

### Contratos e integração

Ao alterar chamadas ao BFF, modelos em `Contracts/*.cs`, PageModels ou formulários relacionados a checkout, pedidos, produto, frete ou busca:

- valide compatibilidade com o contrato canônico do BFF no repositório de referência;
- mantenha propagação de correlação HTTP;
- mantenha idempotência em operações transacionais quando aplicável;
- não mova regra de negócio central para Razor Pages.

## Checklist antes de finalizar alterações frontend

Antes de concluir qualquer mudança visual ou funcional no frontend, valide:

- A tela usa tokens `--marketplace-*` em vez de valores visuais soltos.
- Componentes existentes foram reaproveitados antes de criar classes novas.
- O layout segue Bootstrap e os padrões de página já usados no projeto.
- Estados de vazio, erro, sucesso, warning e loading foram tratados quando aplicável.
- Formulários usam tag helpers, antiforgery padrão e validação consistente.
- Botões de submit demorados usam `data-loading-text`.
- A tela funciona em desktop e mobile.
- Elementos interativos têm texto, label acessível ou contexto semântico suficiente.
- Mudanças de contrato foram conferidas contra o BFF canônico.
- O build continua válido com `dotnet build --no-restore` após restore prévio.
