# Projeto AgroTech - Arquitetura e Solução

## Objetivo do Projeto
O AgroTech é uma solução integrada de gestão agrícola inteligente, projetada para monitorar plantações e safras por meio de sensores IoT, análise de dados ambientais, automação de irrigação e controle de produtividade. O sistema visa conectar campo e gestão, possibilitando decisões em tempo real baseadas em dados precisos.

## Escopo do Sistema
- Backend Java (Spring Boot) para gerenciamento agrícola, usuários e sensores.
- Módulo .NET (C#) para integração com dispositivos, segurança e dashboards.
- Banco de Dados Oracle Cloud para persistência, utilizando Entity Framework Core para mapeamento e migrações.
- Camada IoT com ESP32 para coleta de dados ambientais.
- Aplicativo Mobile com TypeScript/Ionic para produtores e técnicos.
- Módulo de IA Oracle para análise preditiva e alertas.

## Requisitos Funcionais
- Cadastro e autenticação de usuários com validações e tratamento de exceções.
- Registro de propriedades, campos e safras.
- Coleta integrada de dados dos sensores, com dashboard para visualização e alertas visuais baseados em valores de sensores.
- Visualização de relatórios de produtividade e acompanhamento em tempo real pelo aplicativo.
- Alertas automáticos via IA Oracle.
- Gerenciamento de permissões por perfil.

## Requisitos Não Funcionais
- Arquitetura limpa e desacoplada, adotando Clean Architecture com camadas distintas.
- Disponibilidade mínima de 99,5% em nuvem.
- Segurança com autenticação JWT e criptografia AES-256.
- Resposta da API em menos de 300 ms.
- Compatibilidade entre módulos Java, .NET e Oracle.
- Uso de Docker e Oracle Cloud Infrastructure (OCI) para implantação e escalabilidade.

## Arquitetura
Adotamos a Clean Architecture com quatro camadas principais:
- Apresentação (Controllers, Views, DTOs)
- Aplicação (Services, Use Cases)
- Domínio (Entities, Models, Exceções customizadas)
- Infraestrutura (Repositories concretos com EF Core, configuração de migrações, integrações externas)

Essa estrutura facilita a manutenção, testes, independência tecnológica e substituição de frameworks.

## Implementações e Funcionalidades
- Serviços robustos para manipulação de sensores, fazendas, safras e usuários, com tratamento de erros via DomainException.
- Implementação de repositórios concretos com operações CRUD usando Entity Framework Core e repositórios em memória para testes.
- Aplicação web ASP.NET Core MVC com Views responsivas utilizando Bootstrap 5, validadores de formulário e navegação customizada.
- Dashboard de sensores com alertas visuais dinâmicos conforme os valores coletados em tempo real.
- Configuração completa do pipeline ASP.NET Core para roteamento, autenticação, autorização, tratamento de erros e arquivos estáticos.
- DTOs e ViewModels para transferência segura e validada de dados entre camadas.
- Inclusão de rotas personalizadas, layout principal com cabeçalho, rodapé, e navegação adaptativa.
- Integração com serviços Oracle AI para modelagem preditiva e alertas baseados em dados coletados.

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
- Projeto compila e estável, com testes unitários e validações garantindo qualidade do software.

## Tecnologias Principais
- Java 21 + Spring Boot 3
- C# .NET 8 com ASP.NET Core MVC
- Entity Framework Core para persistência e migrações
- Oracle Database Cloud, APEX, AI Services
- TypeScript + Ionic Mobile
- ESP32 (IoT)
- Swagger / OpenAPI 3.0 para documentação da API
- Docker e Oracle Cloud Infrastructure (OCI)
- Bootstrap 5 para frontend responsivo
- JWT e AES-256 para segurança

---

Este README apresenta uma visão atualizada da arquitetura, implementações e funcionalidades da solução AgroTech, destacando a integração entre software, hardware e inteligência artificial para agricultura inteligente.
