# DatabaseClients

## 主要相依套件
```
NETStandard.Library
Microsoft.Extensions.Configuration
Microsoft.Extensions.Configuration.Json
System.Configuration.ConfigurationManager
System.Data.SqlClient
Oracle.ManagedDataAccess.Core
```

## Config設定載入方法
AppSettings.json
```
ConfigurationManager.LoadJsonConfiguration("AppSettings.json");
```
Web.config
```
ConfigurationManager.LoadAppConfiguration();
```