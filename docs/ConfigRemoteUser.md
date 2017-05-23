# Config a Domain User for Powershell

Enable-PSRemoting -Force

Restart-Service WinRM

Test-WsMan <COMPUTER NAME>



# Allowing PowerShell Remoting for standard users with Group Policy

Open the "Group Policy Management Users Editor" 

Remote Management Users
