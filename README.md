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

O **PortSafe** tem como objetivo modernizar o processo de entrega em condomínios, eliminando a necessidade de intervenção do porteiro e proporcionando segurança e praticidade tanto para moradores quanto para entregadores.  

A solução utiliza **armários inteligentes**, integrados a um sistema que valida os dados da entrega, registra a encomenda e notifica o morador via **WhatsApp** com as credenciais de retirada.

### 📖 Definições, Acrônimos e Abreviações

- **PortSafe**: Nome do sistema.  
- **API**: Interface de Programação de Aplicativos.  
- **Armários Inteligentes**: Estrutura física que recebe as entregas com segurança.  
- **Usuários**: Clientes (moradores), Entregadores e Porteiro (em caso de erros).  

### 📚 Referências

- Documentação do .NET  
- Documentação do Google Maps API e Dialogflow (planos gratuitos)  
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

- **Hardware**: Navegadores web (desktop e mobile).  
- **Software**: React (frontend), ASP.NET Core (backend), PostgreSQL (banco).  
- **APIs externas**: Correios, Google Maps, Dialogflow.  
- **Condição**: Hospedagem gratuita via Docker.  

---

## 4. Descrição do Produto

### Principais Funcionalidades

- Rastreamento de entregas (API Correios).  
- Notificações automáticas (WhatsApp).  
- Armários inteligentes com senhas únicas.  
- Validação de destinatário + endereço.  
- Encaminhamento para portaria em caso de erro.  

### Suposições e Dependências

- **Suposições**: Usuários têm acesso à internet.  
- **Dependências**: Integração frontend, backend e APIs externas.  

---

## 5. Requisitos de Alto Nível

### Funcionais

- Login para clientes e porteiro via e-mail/senha.  
- Registro de entrega pelo entregador.  
- Validação do endereço no sistema.  
- Envio de notificações automáticas ao cliente.  
- Abertura automática de armário para entrega/retirada.  

### Não Funcionais

- **Usabilidade**: Interface intuitiva.  
- **Confiabilidade**: APIs respondem em até 5s.  
- **Desempenho**: Até 10 usuários simultâneos.  
- **Segurança**: JWT + HTTPS.  
- **Portabilidade**: Desktop e mobile.  

---

## 6. Qualidade do Produto

- **Usabilidade**: Design responsivo, feedback visual.  
- **Confiabilidade**: Histórico salvo no banco.  
- **Desempenho**: Testes com usuários simulados.  
- **Segurança**: Autenticação JWT + PostgreSQL.  
- **Portabilidade**: Compatível com navegadores modernos.  

---

## 7. Restrições

- Uso exclusivo de ferramentas gratuitas.  
- Escopo limitado (10 clientes + 1 porteiro).  
- Zero custo (projeto acadêmico).  

---

## 8. Riscos

- Limite de APIs gratuitas.  
- Complexidade de integração.  
- Experiência limitada da equipe.  
- Prazo acadêmico reduzido.  

---

## 9. Cronograma

- **Semana 1 (13/08/2025):** Documento de Visão.  
- **Semana 2 (22/08/2025):** Protótipo no Figma + planejamento.  
- **Semana 3-4 (05/09/2025):** Backend + login.  
- **Semana 5-6 (19/09/2025):** Telas cliente/porteiro + APIs.  
- **Semana 7 (26/09/2025):** Notificações + testes iniciais.  
- **Semana 8-9 (10/10/2025):** Chatbot + integração.  
- **Semana 10-11 (24/10/2025):** Artigo científico + testes finais.  
- **Semana 12 (07/11/2025):** Revisão + documentação + apresentação.  

---

## 10. Apêndices

### Glossário

- **Código de Rastreamento**: Identificador único (ex.: AA123456789BR).  
- **Notificação**: Alerta via WhatsApp ou navegador.  
- **Geocodificação**: Conversão de endereço em coordenadas.  

> ### **Diagrama Simplificado**

```text
             +--------------------+
             |  Sistema de Armários|
             +--------------------+
         /|\           /|\           /|\
          |             |             |
          |             |             |
   +-----------+  +-------------+  +-----------+
   | Entregador|  |   Morador   |  |  Portaria |
   +-----------+  +-------------+  +-----------+

        |                 |              |
        |                 |              |
        |--- Identificar destinatário --->|
        |--- Validar endereço ------------>|
        |--- Redigitar dados (opcional) -->|
        |--- Encaminhar à portaria --------> (em caso de erro)
        |--- Confirmar entrega ----------->|
        |--- Guardar encomenda ----------->|
        |<-- Gerar código de entrega ------|
        |<-- Notificação via WhatsApp -----|
                          |
                          |--- Retirar encomenda --->|
```

> ### **Dados Fictícios para Testes**

- **Clientes:** 5 apartamentos (Torre A, Apto 101-105), 5 casas (Quadra 1-3, Casa 1-5).

- **Códigos:** Ex.: AA123456789BR ("Em trânsito"), BB987654321BR ("Entregue").

- **Endereços:** Ex.: "Rua Fictícia, 123, São Paulo, SP" (-23.5505, -46.6333).
