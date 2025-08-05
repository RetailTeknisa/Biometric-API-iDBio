' Obtém o diretório de instalação passado pelo MSI
Dim installDir
installDir = Session.Property("CustomActionData")

' Monta o caminho completo para o arquivo .bat
Dim commandString
commandString = """" & installDir & "install-service.bat" & """"

' Cria o objeto Shell para executar comandos com privilégios
Set objShell = CreateObject("Shell.Application")

' Executa o .bat como Administrador ("runas") e com a janela oculta (0)
objShell.ShellExecute "cmd.exe", "/c " & commandString, "", "runas", 0