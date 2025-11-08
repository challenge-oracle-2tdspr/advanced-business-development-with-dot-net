# Projeto AgroTech - Arquitetura e Solução

## Objetivo do Projeto
O AgroTech é uma solução integrada de gestão agrícola inteligente, projetada para monitorar plantações e safras por meio de sensores IoT, análise de dados ambientais, automação de irrigação e controle de produtividade. O sistema visa conectar campo e gestão, possibilitando decisões em tempo real baseadas em dados precisos.

## Escopo do Sistema
- Backend Java (Spring Boot) para gerenciamento agrícola, usuários e sensores.
- Módulo .NET (C#) para integração com dispositivos, segurança e dashboards.
- Banco de Dados Oracle Cloud para persistência.
- Camada IoT com ESP32 para coleta de dados ambientais.
- Aplicativo Mobile com TypeScript/Ionic para produtores e técnicos.
- Módulo de IA Oracle para análise preditiva e alertas.

## Requisitos Funcionais
- Cadastro e autenticação de usuários.
- Registro de propriedades, campos e safras.
- Coleta integrada de dados dos sensores.
- Visualização de relatórios de produtividade.
- Alertas automáticos via IA.
- Acompanhamento em tempo real pelo aplicativo.
- Gerenciamento de permissões por perfil.

## Requisitos Não Funcionais
- Arquitetura limpa e desacoplada.
- Disponibilidade mínima de 99,5% em nuvem.
- Segurança com autenticação JWT e criptografia AES-256.
- Resposta da API em menos de 300 ms.
- Compatibilidade entre módulos Java, .NET e Oracle.

## Arquitetura
Adotamos a Clean Architecture com quatro camadas principais:
- Apresentação (Controllers, DTOs)
- Aplicação (Services, Use Cases)
- Domínio (Entities, Models)
- Infraestrutura (Repositories, Configurações)

Essa estrutura facilita a manutenção, testes, independência tecnológica e substituição de frameworks.

## Integração IoT e Inteligência Artificial
- Sensores ESP32 coletam temperatura, umidade, pH, luminosidade, chuva e vento em tempo real.
- Dados são enviados via MQTT ou HTTP REST ao backend para armazenamento e análise.
- Oracle AI Services aplicam modelos preditivos para produtividade e alertas.
- Dashboards Oracle APEX e Power BI exibem métricas de desempenho da lavoura.

## Garantia da Qualidade (QA)
- Público-alvo: cooperativas, agrônomos, produtores médios e grandes.
- Solução modular, integrável e acessível, focada em produtores médios.
- Mercado global esperado em US$ 55 bilhões até 2030.
- MVP contempla coleta real de dados, dashboard funcional e integração básica cloud.

## Tecnologias Principais
- Java 21 + Spring Boot 3
- C# .NET 8
- Oracle Database Cloud, APEX, AI Services
- TypeScript + Ionic Mobile
- ESP32 (IoT)
- Swagger / OpenAPI 3.0
- Docker e Oracle Cloud Infrastructure (OCI)

---

Este README apresenta uma visão geral da arquitetura e funcionalidades da solução AgroTech, destacando a integração entre software, hardware e inteligência artificial para agricultura inteligente.
