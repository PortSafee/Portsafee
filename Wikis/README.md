# 📄 Documento de Visão - PortSafe: Entrega Segura e Inteligente

## 👥 Integrantes do Projeto

- Pedro Henrique Dias de Paula Santos  
- Maria Eduarda Claro  
- Milenna Victoria Assis Portella  
- Juliana Fernandes do Nascimento  

---

## 1. Introdução

### 🎯 Objetivo do Documento

Nosso projeto **PortSafe** é uma aplicação web desenvolvida para otimizar o recebimento de entregas em condomínios.  
Este documento define o escopo, as funcionalidades, os usuários, os requisitos e as restrições do sistema, servindo como guia para a equipe de desenvolvimento e para avaliação acadêmica.

### 📌 Escopo do Produto

O **PortSafe** tem como objetivo modernizar o processo de entrega em condomínios, minimizando a necessidade de intervenção do porteiro e proporcionando segurança e praticidade tanto para moradores quanto para entregadores.  

A solução utiliza **armários inteligentes**, integrados a um sistema que:
- Valida os dados do destinatário comparando nome e CEP
- Libera automaticamente armários disponíveis
- Gera códigos únicos de entrega
- Notifica o morador via **email** (com suporte futuro para WhatsApp) com senha e número do armário para retirada

### 📖 Definições, Acrônimos e Abreviações

- **PortSafe**: Nome do sistema.  
- **API**: Interface de Programação de Aplicativos.  
- **Armários Inteligentes**: Estrutura física que recebe as entregas com segurança.  
- **Usuários**: Clientes (moradores), Entregadores e Porteiro (em caso de erros).  

### 📚 Referências

- Documentação do .NET 9.0
- Entity Framework Core (PostgreSQL)
- MailKit/MimeKit para envio de emails
- JWT para autenticação e autorização
- Documentação do Docker para containerização
- Wireframes e arquitetura do projeto (documentos internos da equipe)  

### 🔎 Visão Geral do Documento

O documento apresenta o **posicionamento do produto**, **stakeholders**, **funcionalidades**, **requisitos**, **qualidade**, **restrições**, **riscos** e **cronograma**.  
Ele é a base para desenvolvimento e apresentação do projeto na disciplina.

---

## 2. Posicionamento

### 💡 Oportunidade de Negócio

Com o crescimento do e-commerce, condomínios enfrentam problemas como demora, insegurança e falhas na comunicação.  
O **PortSafe** oferece uma solução gratuita e acessível, melhorando a experiência de moradores, porteiros e entregadores, com potencial para uso real.

### 🚨 Problema a Ser Resolvido

- Longos tempos de espera na portaria.  
- Pacotes extraviados ou recebidos incorretamente.  
- Falta de integração com sistemas de gestão condominial.  
- Dificuldade de localização em condomínios grandes.  

### 🛠️ Descrição do Produto

O **PortSafe** é uma aplicação web que permite:

- Rastrear entregas em tempo real (API Correios).  
- Notificar moradores automaticamente.  
- Armazenar encomendas em armários inteligentes.  

### 📢 Declaração de Posição do Produto

Para **moradores e entregadores de condomínios**, que precisam de um processo de entrega eficiente e seguro, o **PortSafe** é uma aplicação web que otimiza rastreamento, comunicação e localização de entregas, oferecendo conveniência e segurança.  

---

## 3. Stakeholders e Usuários

### Stakeholders

**Porteiro**  

- **Descrição**: Responsável por auxiliar em casos de erro.  
- **Características**: Pode liberar armários e validar entregas manualmente.  

### Usuários

**Cliente (Morador)**  

- **Descrição**: Moradores do condomínio.  
- **Características**: Idade variada, familiaridade básica com tecnologia.  
- **Exemplos**: João Silva (Apto 101, Torre A), Carla Mendes (Quadra 1, Casa 1).  

### Necessidades

- **Cliente**: Rastrear entregas, receber notificações, retirar pacotes.  
- **Porteiro**: Atuar somente em casos de erro.  

### Ambiente Operacional

- **Hardware**: Navegadores web (desktop e mobile), servidores cloud.  
- **Software**: 
  - Frontend: React (em desenvolvimento)
  - Backend: ASP.NET Core 9.0 + Entity Framework Core
  - Banco de Dados: PostgreSQL 17.2
  - Containerização: Docker + Docker Compose
- **Serviços Integrados**: 
  - Gmail SMTP (notificações por email)
  - JWT (autenticação segura)
- **Hospedagem**: Docker (local) com planos de deploy no Render (API) e banco de dados em cloud  

---

## 4. Descrição do Produto

### Principais Funcionalidades

**Autenticação e Gestão de Usuários:**
- Cadastro de moradores e porteiros
- Login com autenticação JWT
- Recuperação de senha via email com código temporário

**Gestão de Condomínios:**
- Criação de condomínios (tipo Casa ou Apartamento)
- Cadastro de unidades (apartamentos ou casas) com endereço completo
- Vinculação de moradores às unidades

**Processo de Entrega:**
- Validação de destinatário por nome e CEP (retorna endereço para confirmação visual)
- Liberação automática de armário disponível após validação
- Geração de código de entrega único
- Senha de acesso de 4 dígitos para o armário
- Detecção de fechamento do armário
- Acionamento de portaria em casos de divergência

**Notificações:**
- Email automático ao morador com número do armário e senha
- Email de boas-vindas no cadastro
- Email com código para reset de senha

**Segurança:**
- Autenticação JWT com tokens seguros
- Senhas criptografadas com hash
- Validação de dados em múltiplas camadas  

### Suposições e Dependências

- **Suposições**: Usuários têm acesso à internet.  
- **Dependências**: Integração frontend, backend e APIs externas.  

---

## 5. Requisitos de Alto Nível

### Funcionais

**RF01 - Autenticação:**
- Login para moradores e porteiros via email/senha
- Geração e validação de token JWT
- Solicitação de reset de senha por email
- Redefinição de senha com código temporário

**RF02 - Gestão de Condomínios:**
- Criar condomínios tipo Casa ou Apartamento
- Listar todos os condomínios cadastrados
- Visualizar detalhes de condomínio específico
- Atualizar informações do condomínio
- Excluir condomínio do sistema

**RF03 - Processo de Entrega:**
- Validar destinatário informando nome e CEP
- Exibir endereço cadastrado para confirmação visual
- Liberar automaticamente armário disponível
- Confirmar fechamento do armário
- Gerar código de entrega único
- Acionar portaria em caso de divergência

**RF04 - Notificações:**
- Enviar email com senha e número do armário ao morador
- Enviar email de boas-vindas no cadastro
- Enviar código de reset de senha por email

**RF05 - Gestão de Armários:**
- Controlar status dos armários (Disponível, Ocupado, EmManutencao, Indisponivel)
- Registrar abertura e fechamento de armários
- Vincular armário à entrega  

### Não Funcionais

- **RNF01 - Usabilidade**: Interface web responsiva compatível com desktop e mobile
- **RNF02 - Confiabilidade**: 
  - Sistema de retry para envio de emails
  - Timeout de 120 segundos para operações de email
  - Histórico completo de entregas armazenado no banco
- **RNF03 - Desempenho**: 
  - API RESTful otimizada com Entity Framework Core
  - Suporte para múltiplos usuários simultâneos
  - Queries otimizadas com Include para carregamento eficiente
- **RNF04 - Segurança**: 
  - Autenticação JWT com chave de 256 caracteres
  - Senhas criptografadas com hash BCrypt
  - Validação de dados em todos os endpoints
  - Tokens com expiração de 60 minutos
- **RNF05 - Manutenibilidade**:
  - Arquitetura em camadas (Controllers, Services, DTOs, Models)
  - Migrations do Entity Framework para versionamento do banco
  - Containerização com Docker para fácil deploy
- **RNF06 - Portabilidade**: 
  - API cross-platform (.NET 9.0)
  - Suporte a PostgreSQL (banco multiplataforma)
  - Docker Compose para deployment padronizado  

---

## 6. Qualidade do Produto

### Implementações de Qualidade

- **Usabilidade**: 
  - DTOs específicos para cada operação
  - Mensagens de erro descritivas
  - Feedback claro em todas as operações
  - Logs detalhados para debugging

- **Confiabilidade**: 
  - Persistência de dados com PostgreSQL
  - Migrations versionadas do Entity Framework
  - Sistema robusto de envio de emails com fallback
  - Tratamento de exceções em todos os endpoints

- **Desempenho**: 
  - Queries otimizadas com carregamento eager loading
  - Índices no banco de dados
  - Validação eficiente de dados

- **Segurança**: 
  - Tokens JWT com assinatura digital
  - Senhas nunca retornadas nas respostas da API
  - Validação de modelos com Data Annotations
  - Proteção contra SQL Injection via Entity Framework

- **Manutenibilidade**:
  - Código organizado em camadas
  - Separação de responsabilidades (Controllers, Services, DTOs)
  - Documentação inline no código
  - Scripts Docker para deploy automatizado  

---

## 7. Restrições

### Restrições Técnicas
- Uso de ferramentas gratuitas e open-source
- Limite de envio de emails do Gmail (500 emails/dia)
- Escopo acadêmico (não destinado a produção comercial inicialmente)

### Restrições de Projeto
- Integração frontend-backend pendente
- Deploy em ambiente de produção pendente
- Testes automatizados não implementados
- API de rastreamento de Correios não integrada
- Integração com WhatsApp planejada mas não implementada

### Restrições de Tempo
- Projeto acadêmico com prazo definido
- Desenvolvimento incremental com entregas semanais  

---

## 8. Riscos

### Riscos Mitigados
- ✅ **Complexidade do Backend**: Estrutura implementada com sucesso
- ✅ **Autenticação**: Sistema JWT implementado e funcional
- ✅ **Banco de Dados**: PostgreSQL configurado com migrations
- ✅ **Email**: Serviço de email Gmail implementado e testado

### Riscos Pendentes
- ⚠️ **Integração Frontend-Backend**: Dependência crítica para conclusão
- ⚠️ **Deploy em Produção**: Render e hospedagem de banco pendentes
- ⚠️ **Limite de Emails Gmail**: 500 emails/dia pode ser restritivo
- ⚠️ **Testes de Carga**: Não realizados em ambiente real
- ⚠️ **Integração WhatsApp**: API planejada mas não implementada

### Plano de Mitigação
- Implementar integração frontend-backend como prioridade
- Configurar deploy no Render com banco PostgreSQL cloud
- Documentar processo de deploy
- Considerar alternativas ao Gmail para maior volume de emails  

---

## 9. Cronograma e Status Atual

### ✅ Concluído (75% do projeto)

**Fase 1 - Planejamento (Agosto 2025):**
- ✅ Documento de Visão
- ✅ Protótipo no Figma
- ✅ Definição de arquitetura

**Fase 2 - Backend (Setembro-Outubro 2025):**
- ✅ Estrutura base da API (.NET 9.0)
- ✅ Models: Usuario, Morador, Porteiro, Condominio, Unidade, Armario, Entrega
- ✅ DTOs para todas as operações
- ✅ AuthController: Cadastro, Login, Reset de Senha
- ✅ CondominioController: CRUD completo
- ✅ EntregaController: Validação, Armário, Confirmação, Portaria
- ✅ AuthService com JWT
- ✅ GmailService (EmailService) com MailKit
- ✅ Migrations do Entity Framework
- ✅ Configuração PostgreSQL
- ✅ Docker e Docker Compose

**Fase 3 - Frontend (Outubro-Novembro 2025):**
- ✅ Todas as telas desenvolvidas
- ⚠️ Integração com backend (em andamento)

### 🔄 Em Andamento (20%)

**Novembro 2025:**
- 🔄 Integração frontend-backend
- 🔄 Testes de integração
- 🔄 Ajustes e refinamentos
- 🔄 Documentação técnica

### 📋 Pendente (5%)

**Novembro-Dezembro 2025:**
- ⏳ Deploy da API no Render
- ⏳ Hospedagem do banco PostgreSQL
- ⏳ Deploy do frontend
- ⏳ Testes finais em produção
- ⏳ Documentação de deploy
- ⏳ Apresentação final

### 🎯 Progresso Geral: 75%  

---

## 10. Apêndices

### Glossário

**Termos do Sistema:**
- **Código de Entrega**: Identificador alfanumérico único de 6 caracteres gerado para cada entrega (ex.: ABCDEF)
- **Senha de Acesso**: Código numérico de 4 dígitos gerado automaticamente para abrir o armário (ex.: 1234)
- **Notificação**: Email automático enviado ao morador via Gmail SMTP
- **Validação de Destinatário**: Processo de verificação de nome e CEP com retorno do endereço cadastrado
- **Status do Armário**: Estados possíveis: Disponivel, Ocupado, EmManutencao, Indisponivel
- **Status da Entrega**: Estados: AguardandoValidacao, AguardandoArmario, Armazenada, Retirada, ErroValidacao, RedirecionadoPortaria
- **JWT (JSON Web Token)**: Token de autenticação com expiração de 60 minutos
- **Reset de Senha**: Código temporário de 6 caracteres enviado por email, válido por 30 minutos

**Entidades do Sistema:**
- **Morador**: Usuário residente do condomínio com CPF, telefone e unidade vinculada
- **Porteiro**: Usuário administrativo responsável por gerenciar condomínios e auxiliar em casos de erro
- **Condomínio**: Estrutura que pode ser tipo "Casa" ou "Apartamento"
- **Unidade**: Pode ser UnidadeCasa (com rua, número, quadra, CEP) ou UnidadeApartamento (com torre, andar, número)
- **Armário**: Estrutura física numerada que armazena entregas temporariamente
- **Entrega**: Registro completo de uma encomenda incluindo destinatário, armário, códigos e status  

> ### **Diagrama Simplificado do Fluxo de Entrega**

```text
┌─────────────────────────────────────────────────────────────────┐
│                     SISTEMA PORTSAFE                            │
│  (API Backend + Armários Inteligentes + Notificações Email)    │
└─────────────────────────────────────────────────────────────────┘
         ▲                    ▲                    ▲
         │                    │                    │
         │                    │                    │
    ┌────────┐           ┌─────────┐         ┌──────────┐
    │Entrega-│           │ Morador │         │ Porteiro │
    │  dor   │           │         │         │          │
    └────────┘           └─────────┘         └──────────┘

    FLUXO DE ENTREGA:
    1. Entregador digita nome + CEP na tela
       ↓
    2. Sistema retorna endereço cadastrado
       ↓
    3. Entregador confirma compatibilidade visual
       ↓
    4. Sistema LIBERA ARMÁRIO automaticamente
       ↓
    5. Entregador deposita encomenda
       ↓
    6. Entregador fecha armário
       ↓
    7. Sistema exibe "ENTREGA FINALIZADA" + CÓDIGO
       ↓
    8. Sistema envia EMAIL ao morador:
       - Número do armário
       - Senha de acesso (4 dígitos)
       ↓
    9. Morador recebe email e retira encomenda

    ALTERNATIVA (em caso de erro):
    - Sistema aciona portaria
    - Porteiro auxilia presencialmente
```

> ### **Dados para Testes**

**Estrutura de Testes:**

**Condomínios:**
- Tipo Casa: "Residencial Jardim Verde"
- Tipo Apartamento: "Edifício Vista Alegre"

**Unidades Casa (exemplo):**
- Rua: "Rua das Flores", Número: 123, Quadra: 1, CEP: "01234-567"
- Rua: "Rua dos Lírios", Número: 45, Quadra: 2, CEP: "01234-568"

**Unidades Apartamento (exemplo):**
- Torre: "A", Andar: 10, Número: 101, CEP: "01234-567"
- Torre: "B", Andar: 5, Número: 52, CEP: "01234-568"

**Moradores (exemplo):**
- Nome: "João Silva", Email: "joao@example.com", CPF: "123.456.789-00", Telefone: "(11) 98765-4321"
- Nome: "Maria Santos", Email: "maria@example.com", CPF: "987.654.321-00", Telefone: "(11) 91234-5678"

**Armários:**
- Número: "001" até "010" (Status: Disponivel/Ocupado)

**Códigos Gerados:**
- Código de Entrega: 6 caracteres alfanuméricos (ex.: "ABC123", "DEF456")
- Senha de Acesso: 4 dígitos (ex.: "1234", "5678")
- Código Reset Senha: 6 caracteres alfanuméricos (ex.: "RST789")

**Credenciais de Teste:**
- Email Porteiro: porteiro@example.com / Senha: SenhaSegura123
- Email Morador: morador@example.com / Senha: SenhaSegura123
