
```
LIB CONNECT TO 'test';

SQL PSEXECUTE()
Get-ChildItem -Force C:\ | Select-Object Name, CreationTime;
```
Result

![dirlist](images/PS_Example1_Result.PNG)
