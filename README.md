# API de Serviço para Leitor Biométrico iDBio

## 1. Visão Geral

A **API Biométrica iDBio** é uma aplicação .NET 8 que roda como um serviço local do Windows. Seu principal objetivo é servir como uma ponte de comunicação (bridge) entre aplicações web (como o Teknisa Retail) e o leitor biométrico **iDBio da Control ID** conectado fisicamente a um computador.

A solução expõe endpoints HTTP RESTful que permitem a uma página web, de qualquer origem, solicitar a captura de uma impressão digital e receber o template biométrico correspondente para processamento externo (ex: salvar em um banco de dados).

A aplicação é construída com C# e ASP.NET Core, empacotada como um serviço autocontido (self-contained) e distribuída através de um instalador MSI para facilitar a instalação no ambiente do cliente final.

## 2. Requisitos

Os requisitos são divididos entre o ambiente de execução (cliente final) e o ambiente de desenvolvimento.

### 2.1. Requisitos para Execução (Cliente Final)

* **Sistema Operacional:** Windows 10 ou superior (versões x86 ou x64).
* **Hardware:**
    * Leitor biométrico iDBio (Control ID).
    * Uma porta USB livre.
* **Driver:** O driver oficial do Windows para o leitor iDBio **precisa** ser instalado.
    * **Link para Download:** [https://www.controlid.com.br/idbio/windows_driver.zip](https://www.controlid.com.br/idbio/windows_driver.zip)
* **Framework .NET:** **Nenhum.** A aplicação é autocontida e inclui todo o runtime necessário para sua execução. O cliente não precisa instalar nenhum pacote do .NET.

### 2.2. Requisitos para Desenvolvimento

* **SDK:** .NET 8 SDK.
* **IDE:** Visual Studio 2022 (Community Edition ou superior).
* **Extensão do Visual Studio:** [Microsoft Visual Studio Installer Projects 2022](https://marketplace.visualstudio.com/items?itemName=MicrosoftVisualStudioInstallerProjects.MicrosoftVisualStudioInstallerProjects2022).
* **Bibliotecas do Fabricante:** Os arquivos `CIDBio.dll` (wrapper .NET) e `libcidbio.dll` (biblioteca nativa) devem estar presentes na raiz do projeto `BiometricApi`.

## 3. Estrutura do Projeto

O projeto é organizado em uma solução com a API principal e o projeto de instalação.

```
.
├── BiometricApi/                  # Projeto principal da API ASP.NET Core
│   ├── Controllers/
│   │   └── BiometricController.cs # Define os endpoints HTTP (ex: /capture)
│   ├── Properties/
│   │   └── launchSettings.json    # Configurações de execução para desenvolvimento
│   ├── Services/
│   │   └── BiometricService.cs    # Contém a lógica de negócio e comunicação com a DLL
│   ├── BiometricApi.csproj        # Arquivo de projeto C# (.NET 8)
│   ├── CIDBio.dll                 # Wrapper .NET fornecido pelo fabricante
│   ├── libcidbio.dll                # DLL nativa C/C++ do fabricante
│   ├── Program.cs                 # Ponto de entrada da aplicação, configuração do serviço e CORS
│   └── appsettings.json           # Configurações da aplicação (ex: porta do servidor)
```

## 4. Funcionamento

O fluxo de uma captura de digital ocorre da seguinte maneira:

1.  **Requisição:** Uma aplicação web (rodando em qualquer navegador) faz uma chamada `POST` para um dos endpoints da API, como `http://localhost:5170/api/biometric/capture-template`.
2.  **Controller:** O `BiometricController` recebe a requisição. Ele não contém lógica de hardware, apenas delega a tarefa para o `BiometricService`.
3.  **Service:** O `BiometricService`  executa os passos para comunicação com o hardware:
    a.  Chama `CIDBio.Init()` para abrir a comunicação com o leitor.
    b.  Chama `idbio.SetParameter()` para configurar parâmetros como o timeout de captura.
    c.  Chama `idbio.CaptureImageAndTemplate()` para ativar o leitor e solicitar a digital do usuário.
    d.  Converte os dados recebidos (como a imagem) para formatos amigáveis para a web (Base64).
    e.  Chama `CIDBio.Terminate()` dentro de um bloco `finally` para garantir que a conexão com o hardware seja sempre encerrada, mesmo em caso de erro.
4.  **Resposta:** O `Service` retorna os dados para o `Controller`.
5.  **Controller:** O `Controller` formata os dados em uma resposta JSON padronizada e a envia de volta para a aplicação web com um código de status HTTP apropriado (200 OK para sucesso, 400 Bad Request para erros de captura, etc.).

### 4.1 Rodando Localmente

Para rodar a API localmente durante o desenvolvimento:
1. Abra uma janela de terminal na pasta raíz do projeto `BiometricApi`.
2. Execute o comando:
   ```sh
   dotnet run
   ```
3. A API estará disponível em `http://localhost:5170` (ou outra porta configurada no `appsettings.json`).

## 5. Publicação e Geração do Instalador (.msi)

Este guia detalha o processo completo para compilar a API, gerar os arquivos finais e empacotá-los em um instalador `.msi` profissional e 100% automático para distribuição ao cliente final.

### 5.1. Pré-requisitos de Ferramentas

Antes de começar, garanta que seu ambiente de desenvolvimento tenha:
1.  **Visual Studio 2022** (versão Community ou superior).
2.  A extensão **Microsoft Visual Studio Installer Projects 2022**. Para instalar:
    * No Visual Studio, vá em `Extensions > Manage Extensions`.
    * Busque por `Microsoft Visual Studio Installer Projects 2022` e instale.
    * Reinicie o Visual Studio após a instalação.

### 5.2. Etapa 1: Publicar a Aplicação (Terminal)

Este passo crucial compila a API e agrupa todos os arquivos necessários para uma execução autocontida, eliminando a necessidade de o cliente instalar o .NET.

1.  Abra um terminal (PowerShell ou CMD).
2.  Navegue até a pasta raiz do projeto da API (ex: `C:\..._seu_projeto_\BiometricApi`).
3.  Execute o seguinte comando:
    ```sh
    dotnet publish -c Release -r win-x86 --self-contained true
    ```
    Isso criará uma pasta `publish` dentro de `bin\Release\net8.0\win-x86\`, contendo a versão final da sua aplicação.

### 5.3. Etapa 2: Criar o Projeto de Instalação (Visual Studio)

Se você ainda não tem, crie o projeto que irá gerar o `.msi`.

1.  Abra sua solução (`.sln`) no Visual Studio.
2.  No painel **Gerenciador de Soluções** (Solution Explorer), clique com o botão direito na sua Solução.
3.  Selecione `Adicionar > Novo Projeto...`.
4.  Busque pelo template **"Setup Project"**, selecione-o, dê um nome (ex: `BiometricApi.Installer`) e crie o projeto.

### 5.4. Etapa 3: Configurar o Projeto de Instalação

Esta é a fase mais detalhada, onde definimos o conteúdo e o comportamento do instalador.

#### A. Propriedades Gerais do Projeto

1.  No Gerenciador de Soluções, clique no projeto `BiometricApi.Installer`.
2.  Pressione a tecla **F4** para abrir a janela de **Propriedades**.
3.  Preencha os seguintes campos para um resultado profissional:
    * **Author**: `Teknisa Software`
    * **Manufacturer**: `Teknisa Software` (Este nome será usado na pasta de instalação, ex: `C:\Program Files (x86)\Teknisa Software`).
    * **ProductName**: `API Biometrica iDBio` (Este nome aparecerá em "Adicionar ou remover programas").

#### B. Configurar o Sistema de Arquivos

Aqui definimos quais arquivos serão copiados para a máquina do cliente.

1.  Clique com o botão direito no projeto `BiometricApi.Installer` e vá em `Exibir > Sistema de Arquivos`.
2.  **Adicionar o Conteúdo da Pasta `publish`:**
    * No Windows Explorer, navegue até a sua pasta de publicação (ex: `...\BiometricApi\bin\Release\net8.0\win-x86\publish\`).
    * Selecione **todos os arquivos e pastas** dentro dela (Ctrl + A).
    * **Arraste e solte** todos esses arquivos diretamente sobre o nome **"Pasta do Aplicativo" (Application Folder)** na janela "Sistema de Arquivos" do Visual Studio.
3.  **Adicionar o `powershell.exe`:**
    * Ainda na janela "Sistema de Arquivos", clique com o botão direito em **"Pasta do Aplicativo"** e selecione `Adicionar > Arquivo...`.
    * Navegue até `C:\Windows\System32\WindowsPowerShell\v1.0\`.
    * Selecione o arquivo `powershell.exe` e clique em Abrir.

#### C. Configurar as Ações Personalizadas (Instalação Automática)

Esta configuração fará com que o serviço seja instalado e desinstalado automaticamente, sem a necessidade de scripts `.bat`.

1.  Clique com o botão direito no projeto `BiometricApi.Installer` e vá em `Exibir > Ações Personalizadas`.
2.  **Ação de Instalação (`Commit`):**
    * Clique com o botão direito em **`Commit`**, selecione `Adicionar Ação Personalizada...`.
    * Na janela, navegue até a `SystemFolder` e selecione **`powershell.exe`**.
    * Selecione a ação `powershell.exe` recém-adicionada e pressione **F4** para abrir suas **Propriedades**.
    * No campo **`Arguments`**, cole o seguinte comando:
      ```
      -WindowStyle Hidden -ExecutionPolicy Bypass -Command "& { Start-Process -FilePath \"[TARGETDIR]install-service.bat\" -Verb RunAs -Wait -WindowStyle Hidden }"
      ```
      * Este comando executa o script de instalação do serviço em segundo plano, garantindo que o usuário não veja janelas de console durante a instalação.
3.  **Ação de Desinstalação (`Uninstall`):**
    * Clique com o botão direito em **`Uninstall`**, adicione uma Ação Personalizada e selecione **`powershell.exe`** da `SystemFolder`.
    * Nas **Propriedades (F4)** desta ação, configure o campo **`Arguments`**:
      ```
      -WindowStyle Hidden -ExecutionPolicy Bypass -Command "& { Start-Process -FilePath \"[TARGETDIR]uninstall-service.bat\" -Verb RunAs -Wait -WindowStyle Hidden }"
      ```

### 5.5. Etapa 4: Gerar o Instalador `.msi` Final

1.  Na barra de ferramentas principal do Visual Studio, mude a configuração da solução de `Debug` para **`Release`**.
2.  No Gerenciador de Soluções, clique com o botão direito no projeto `BiometricApi.Installer` e selecione **"Recriar" (Rebuild)**.
3.  Após a compilação, o arquivo `.msi` final e pronto para distribuição estará na pasta `BiometricApi.Installer\Release\`. Para abrir a pasta, clique com o botão direito no projeto `BiometricApi.Installer` e selecione `Abrir Pasta no Gerenciador de Arquivos`.

## 6. Utilização e Conclusão
Após seguir todos os passos acima, você terá um instalador `.msi` que pode ser distribuído para clientes finais. Eles poderão instalar a API Biométrica iDBio com apenas alguns cliques, sem precisar se preocupar com dependências do .NET ou configuração manual. 

Depois de instalado, o serviço estará rodando em segundo plano, pronto para receber requisições HTTP (porta padrão: `5170`) e capturar impressões digitais através do leitor iDBio.