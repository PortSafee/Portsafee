# Documento de Visão - PortSafe: Entrega Segura e Inteligente

## Integrantes do Projeto: Pedro Hennrique Dias de Paula Santos, MAria Eduarda Claro, Milenna Victoria Assis Portella, Juliana Fernandes do Nascimento

## **Introdução**

> ### **Objetivo do Documento**
Nosso projeto **PortSafe**, é uma aplicação web desenvolvida para otimizar o recebimento de entregas em condomínios. Ele define o escopo, as funcionalidades, os usuários, os requisitos e as restrições do projeto, servindo como guia para a equipe de desenvolvimento e para a avaliação acadêmica.

> ### **Escopo do Produto**
O **PortSafe** é uma solução web que facilita a gestão de entregas em condomínios de apartamentos e casas, integrando rastreamento de pacotes, notificações automáticas, localização por mapas e interação via chatbot. O sistema permite que moradores acompanhem suas entregas, porteiros registrem chegadas e entregadores localizem endereços com precisão, tudo sem custos, utilizando ferramentas gratuitas.

> ### **Definições, Acrônimos e Abreviações**
- **PortSaf***: Nome do sistema.

- **API:** Interface de Programação de Aplicativos.

- **SRO (Sistema de Rastreamento de Objetos):** Sistema de Rastreamento de Objetos (Correios).

- **Usuários:** Clientes (moradores) e Porteiro.

> ### **Referências**
- Documentação da API dos Correios (SRO Teste).

- Documentação do Google Maps API, Dialogflow (planos gratuitos).

- Wireframe e arquitetura do projeto (documentos internos da equipe).

> ### **Visão Geral do Documento**
Este documento detalha o posicionamento do produto, os stakeholders, as funcionalidades, os requisitos, as características de qualidade, as restrições, os riscos e o cronograma. Ele serve como base para o desenvolvimento e a apresentação do projeto na disciplina.

---

## **Posicionamento**

> ### **Oportunidade de Negócio**
Com o crescimento do comércio eletrônico, a demanda por entregas em condomínios aumentou, mas muitos enfrentam problemas como demora, insegurança e falta de comunicação. O **PortSafe** oferece uma solução gratuita e acessível para melhorar a experiência de moradores, porteiros e entregadores, com potencial para aplicação em condomínios reais.

> ### **Problema a Ser Resolvido**
Moradores de condomínios enfrentam dificuldades para receber entregas devido à falta de integração entre aplicativos de entrega e sistemas de gestão condominial. Isso resulta em longos tempos de espera, pacotes extraviados, insegurança e sobrecarga na portaria. Além disso, entregadores têm dificuldade em localizar endereços em condomínios grandes.

> ### **Descrição do Produto**
O **PortSafe** é uma aplicação web que permite:
- Rastrear entregas em tempo real via API dos Correios.

- Interagir com um chatbot para consultar status e horários.

- Receber notificações automáticas quando pacotes chegam.

- Localizar torres/apartamentos ou casas com mapas interativos.

> ### **Declaração de Posição do Produto**
Para **moradores e porteiros de condomínios**, que **precisam de um processo de entrega eficiente e seguro**, o **PortSafe** é uma **aplicação web** que **otimiza o rastreamento, comunicação e localização de entregas**, proporcionando conveniência e segurança.

---

## **Stakeholders e Usuários**

> ### **Identificação dos Stakeholders**
- **Porteiro:**
  - **Descrição:** Funcionário do condomínio que gerencia entregas.

  - **Características:** Usa o sistema para registrar pacotes e confirmar localizações.


> ### **Identificação dos Usuários**
- **Cliente (Morador):**
  - **Descrição:** Moradores de condomínios.

  - **Características:** Idade variada, familiaridade básica com tecnologia, precisam rastrear entregas e receber notificações.

  - **Exemplo:** João Silva (Apto 101, Torre A), Carla Mendes (Quadra 1, Casa 1).

> ### **Necessidades dos Usuários e Stakeholders**
- **Clientes:** Acompanhar entregas, receber notificações, interagir com chatbot, localizar endereços.

- **Porteiro:** Registrar entregas de forma simples, visualizar histórico, confirmar localizações.

> ### **Ambiente Operacional**
- **Hardware:** Navegadores web (desktop e mobile) para clientes e porteiro.

- **Software**: React (frontend), ASP.NET Core (backend), PostgreSQL (banco), APIs externas (Correios, Dialogflow, Google Maps).

- **Condições:** Conexão à internet para acesso às APIs e hospedagem em plataformas gratuitas (Docker).

---

## **Descrição do Produto**

> ### **Principais Funcionalidades**
1. **Rastreamento de Entregas:** Consulta de status via API dos Correios.

2. **Chatbot:** Respostas a perguntas sobre status e horários via Dialogflow.

3. **Notificações:** Alertas e simulação de SMS.

4. **Mapas:** Localização de torres/apartamentos ou casas via Google Maps API.

> ### **Suposições e Dependências**
- **Suposições:**
  - APIs externas (Correios, Google Maps, Dialogflow) estarão disponíveis nos planos gratuitos.

  - Usuários têm acesso à internet.

- **Dependências:**
  - Configuração de contas gratuitas para APIs e hospedagem.

  - Integração bem-sucedida entre frontend, backend e APIs.

> ### **Limitações**
- Suporta apenas entregas rastreadas pelos Correios.

- Notificações SMS são simuladas na interface e serão enviadas.

- Limite de requisições nas APIs gratuitas (ex.: Google Maps, Dialogflow).

- Escopo restrito a 10 clientes e 1 porteiro para testes.

---

## **Requisitos de Alto Nível**

> ### **Requisitos Funcionais**
1. O sistema deve permitir login de clientes e porteiro via e-mail/senha.

2. Clientes devem inserir códigos de rastreamento e visualizar status.

3. O porteiro deve registrar entregas e associá-las ao clientes.

4. O sistema deve enviar notificações quando entregas forem registradas.

5. O chatbot deve responder perguntas sobre status e instruções.

6. O sistema deve exibir mapas com localizações de apartamentos/casas.

> ### **Requisitos Não Funcionais**
1. **Usabilidade:** Interface intuitiva, com navegação clara.

2. **Confiabilidade:** APIs externas devem responder em até 5 segundos.

3. **Desempenho:** Suporta (até 10 usuários simultâneos sem falhas).

4. **Segurança:** Proteger dados com autenticação e HTTP.

5. **Portabilidade:** Funcionar em navegadores desktop e mobile.

---

## **Qualidade do Produto**

> ### **Usabilidade**
- Interfaces simples, com campos claros e feedback visual (ex.: botão "Rastrear" muda de cor).

- Chatbot com respostas naturais e fáceis de entender.

- Design responsivo para dispositivos móveis.

> ### **Confiabilidade**
- APIs externas são testadas com dados fictícios para garantir respostas consistentes.

- Banco de dados armazena histórico para evitar perda de informações.

> ### **Desempenho**
- Resposta rápida para rastreamento e notificações.

- Suporta (em teste) 10 usuários simulados sem lentidão.

> ### **Segurança**
- Autenticação via JWT para proteger acesso.

- Dados sensíveis (e-mails, endereços) armazenados com segurança no PostgreSQL.

> ### **Portabilidade**
- Compatível com navegadores modernos (Chrome, Firefox, Safari) em desktop e mobile.

---

## **Restrições**
- **Tecnológicas:** Uso exclusivo de ferramentas gratuitas (React, ASP.NET Core, PostgreSQL, JWT, APIs com planos gratuitos).

- **Orçamentárias:** Zero custo, devido ao contexto acadêmico.

- **Legais:** Conformidade com políticas das APIs (ex.: limites de requisições do Google Maps).

- **Escopo:** Limitado a 10 clientes, 1 porteiro e funcionalidades definidas.

---

## **Riscos**
1. **Indisponibilidade de APIs:** APIs gratuitas podem atingir limites de requisições.
   - **Solução:** Usar dados fictícios e armazenar coordenadas fixas no banco.

2. **Complexidade de Integração:** Dificuldade em conectar frontend, backend e APIs.
   - **Solução:** Testar integrações isoladamente antes da integração final.

3. **Falta de Experiência:** Time iniciante pode enfrentar desafios técnicos.
   - **Solução:** Dividir tarefas claras e usar documentação oficial das ferramentas.

4. **Prazo Curto:** Cronograma acadêmico pode limitar testes.
   - **Solução:** Priorizar funcionalidades principais e realizar testes incrementais.

## **Cronograma de Marcos**
- **Semana 1 (até 13/08/2025):** Elaboração da Proposta Inicial e Documento de Visão.

- **Semana 2 (até 22/08/2025):** Criação do protótipo no Figma, planejamento das sprints e configuração inicial do monorepo.

- **Semana 3-4 (até 05/09/2025):** Desenvolvimento do backend, tela de login e início das telas do cliente e porteiro.

- **Semana 5-6 (até 19/09/2025):** Finalização das telas do cliente e porteiro, integração com APIs externas (Correios, Google Maps), início do chatbot.

- **Semana 7 (até 26/09/2025):** Implementação do sistema de notificações e testes iniciais com dados fictícios.

- **Semana 8-9 (até 10/10/2025):** Finalização do chatbot, testes de integração e início da redação do artigo científico.

- **Semana 10-11 (até 24/10/2025):** Finalização do artigo científico, testes completos (unitários e usuário) e integração final do frontend com o backend.

- **Semana 12 (até 07/11/2025):** Revisão final, correção de bugs, finalização da documentação e preparação da apresentação.

## **Apêndices**
> ### **Glossário**
- **Código de Rastreamento:** Identificador único de um pacote (ex.: AA123456789BR).

- **Notificação:** Alerta enviado ao navegador ou dispositivo.

- **Geocodificação:** Conversão de endereços em coordenadas (latitude/longitude).

> ### **Diagrama Simplificado**
```
[Cliente/Porteiro] --> [Frontend: React]
                        | HTTP
                        |
[JWT Auth] <--> [Backend: ASP.NET Core]
                        | Services
                        |
[PostgreSQL] <--> [Correios, Dialogflow, Google Maps]
```

> ### **Dados Fictícios para Testes**
- **Clientes:** 5 apartamentos (Torre A, Apto 101-105), 5 casas (Quadra 1-3, Casa 1-5).

- **Códigos:** Ex.: AA123456789BR ("Em trânsito"), BB987654321BR ("Entregue").

- **Endereços:** Ex.: "Rua Fictícia, 123, São Paulo, SP" (-23.5505, -46.6333).
