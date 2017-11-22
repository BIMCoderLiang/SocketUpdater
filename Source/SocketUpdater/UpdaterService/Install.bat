%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe UpdaterService.exe
Net Start SocketService
sc config SocketService start= auto
pause
