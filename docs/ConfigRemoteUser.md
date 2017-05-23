# Setup a Domain User for Powershell

## Allowing PowerShell Remoting for standard users with Group Policy

1. Open the "Active Directory User and Computer"
2. Select the user to be allowed for Powershell Remoting.
3. In the context menu (right click), select "Properties".
4. Select the "Member of" tab and click the button "Add".
5. Find the "Remote Management Users" and click "OK".

## Allowing PowerShell Remoting

1. Open a Powershell Window and write following command to enable the remoting.
```
Enable-PSRemoting -Force
```
2. Restart the remoting service with the following command.
```
Restart-Service WinRM
```
3. You can test the connection with following command.
```
Test-WsMan <COMPUTER NAME>
```
