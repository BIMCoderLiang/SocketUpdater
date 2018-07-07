## SocketUpdater ReadMe

A Revit Plug-In Updater Project Based On Multi-Thread Task Socket

* * *

### Project Source

Most of clients don't want to uninstall the old,then install the new one when a revit plug-in releases very frequently. Therefore, I started this project and hope to solve this problem.    

* * *

### Main Content

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/MainContent.png)

*   Client:
    A **dll** called **UpdaterClient** which used to be called in our plug-in.
    A **exe** called **UpdaterRestart** which is included in our plug-in and it used to replace old files with new ones and restart Autodesk Revit.
*   Common:
    A **dll** called **UpdaterShare** which is used to be called both in Client and Server.
*   Server:
    A **Windows service** called **UpdaterService** which runs Server Socket to send update files to Client.
    A **Webapi service** called **UpdaterWebServer** which returns some brief infos about update files (file latestversion, file length and file Md5 value) to Client.
*   Tool:
    A **exe** called **TestTool** which runs whole workflow.

* * *

### WorkFlow

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/Workflow.png)

* * *

### Each Part

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/UpdaterClient.png)

*   **BootUtils**:
    this class is used to create .lnk in Startup folder which can makes **UpdaterRestart** runs when computer boots.
*   **RegistryUtils**:
    this class is used to Read, Write, Create and Delete product Registry info.
*   **RequestInfoUtils**:
    this class is used to send packed Registry info to Server and receive update file info, such as its version, length, Md5 value.
*   **ClientSocket**:
    this class is used to send download file request and receive data packets from Server,  and finally combine data packets.

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/UpdateFile.png)

* * *

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/UpdaterRestart.png)

*   **ExampleFile (Not be called in program)**
    **config.xml**: this xml file is inclueded in UpdateFile.zip which can determined the file location.
    **updateInfo.xml**: this xml file records the client decisions when he/she completes download files, whether restart revit now or next computer boot time.
    While it records product Registry info.

*   **ProcessUtils**:
    this class is used to kill Revit process and start **UpdaterRestart** exe.

*   **ReplaceFilesUtils**:
    this class is used to replace old files with new ones.
*   **ZipUtils**:
    this class is used to Zip files and Unzip .zip file.
*   **app.mainfest**:
    this file is used to get high control for Registry operations.
*   **ExecuteUpdate**:
    this console program is used to execute final update operations, such as unzip UpdateFile.zip, cover old files, update Registry info etc.
*   **Restart.vbs**:
    this vbs script is only used when Client chosen update next computer boots time.

* * *

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/Common.png)

*   **GlobalSetting Folder**:
    Define some shared parameters both in Client and Server.
*   **Model Folder**:
    Define some data types and socket packet.
*   **Utility Folder**:
    Some common functions which used in program.

* * *

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/UpdaterService.png)

*   **ExampleFile**:
    a example update file which is used to test update project.
*   **ExecuteProgram**:
    a Windows service main program.
*   **Install.bat**:
    a bat which starts SocketService.
*   **ServerSocket**:
    a class which runs Server Socket service.
*   **Uninstall.bat**:
    a bat which stops and deletes SocketService.

* * *

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/UpdaterWebServer.png)

*   **ApiController Folder**:
    the controller returns update file info to Client.

* * *

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/TestTool.png)

The TestTool completed with DevExpress MVVM Mode.

*   **Preparation: Please run Install.bat to start SocketService in UpdaterService before TestTool**

*   **Step1: Generate Local Registry Info**

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/Step1.png)

*   **Step2: Get Update File Info**

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/Step2.png)

*   **Step3: Download Update Zip File**

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/Step3.png)

*   **Step4: Restart Autodesk Revit**

![image](https://github.com/airforce094/SocketUpdater/raw/master/Images/Step4.png)

* * *

### Contact Me

**E-mail: bim.frankliang@foxmail.com** 

**QQ:1223475343**
