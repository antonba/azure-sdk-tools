# Windows Azure PowerShell

This repository contains a set of PowerShell cmdlets for developers and administrators to develop, deploy and manage 
Windows Azure applications.

For documentation on how to build and deploy applications to Windows Azure please see the [Windows Azure Developer Center](http://www.windowsazure.com/en-us/develop). For comprehensive documentation on the developer cmdlets see [How to use Windows Azure PowerShell](http://www.windowsazure.com/en-us/develop/nodejs/how-to-guides/powershell-cmdlets/). For comprehensive documentation on the full set of Windows Azure cmdlets see [Windows Azure Management Center](http://go.microsoft.com/fwlink/?linkID=254459&clcid=0x409).

# Cmdlets Features

* Account
  * Get and import Azure publish settings
* Subscription
  * Manage Azure subscription
  * Manage AffinityGroup
* Website
  * Manage website, such as CRUD, start and stop.
  * Diagnostics
      * Log streaming
      * Save log
* Cloud service
  * Create scaffolding for cloud service and role. Role support includes Node.js, PHP, Django and Cache.
  * Manage cloud service and role, such as CRUD, start and stop.
  * Enable/Disable remote desktop.
  * Start/Stop Azure emulator.
  * Manage certificate.
* Storage
  * Manage storage account and access key.
  * Manage storage container and blob.
  * Copy storage blob.
  * Manage storage table.
  * Manage storage queue.
* SQL Azure
  * CRUD support for database server, database and firewall rule
* Service Bus
  * Manage service bus namespaces.
* VM
  * Manage VM, such as CRUD, import/export and start/stop/restart.
  * Manage VM image, such as CRUD.
  * Manage disk, such as CRUD.
  * Manage VM endpoint, such as CRUD.
  * Get/Set VM sub net.
  * Manage certificate and SSH key.
  * PowerShell remoting
* Deployment
  * Manage deployment, such as CRUD, move, upgrade and restore.
  * Get/Create DNS settings of a deployment.
* VNet
  * Manage virtual network config, connection and gateway.
* Store
  * View available Windows Azure Store Add-On plans.
  * Purchase, view, upgrade and remove Windows Azure Store Add-On instances.
* Utility
  * Test whether a name is avaiable. Currently support cloud service name, storage account name and service bus namespace name.
  * Get the list of geo locations supported by Azure.
  * Get the list of OS supported by Azure.
  * Direct you to Azure portal.

For detail descriptions and examples of the cmdlets, type
* ```help azure``` to get all the cmdlets.
* ```help node-dev``` to get all Node.js development related cmdlets.
* ```help php-dev``` to get all PHP development related cmdlets.
* ```help python-dev``` to get all Python development related cmdlets.
* ```get-help <cmdlet name>``` to get the details of a specific cmdlet.

# Getting Started

## Install from Microsoft Web Platform Installer

1. Install [Microsoft Web Platform Installer](http://www.microsoft.com/web/downloads/platform.aspx).
2. Open Microsoft Web Platform Installer and search for __Windows Azure PowerShell__.
3. Install.

You can also find the list of previous versions at [Downloads](https://github.com/WindowsAzure/azure-sdk-tools/wiki/Downloads)

## Download Source Code

To get the source code of the SDK via git just type:
```git clone https://github.com/WindowsAzure/azure-sdk-tools.git<br/>cd ./azure-sdk-tools```

## Install Prerequisites

* [Windows Azure SDK](http://www.microsoft.com/windowsazure/sdk/)
* 0.6.9 or less
  * [Windows PowerShell 2.0](http://technet.microsoft.com/en-us/scriptcenter/dd742419)
* 0.6.10 or more
  * [Windows PowerShell 3.0](http://www.microsoft.com/en-us/download/details.aspx?id=34595)
* [WiX](http://wix.sourceforge.net/) (Only needed if you want to build the setup project)

### Node.js Prerequisites (developer only)

* [Node.js](http://nodejs.org/)
* [IISNode](https://github.com/tjanczuk/iisnode)

### PHP Prerequisites (developer only)

* [PHP](http://php.iis.net/)

## Configure PowerShell to automatically load commandlets

1. Create a folder inside your user's Documents folder and name it __WindowsPowerShell__

2. Inside that folder create a file called __Microsoft.PowerShell_profile.ps1__

3. Edit the file in a text editor and add the following contents

   ```Import-Module PATH_TO_AZURE-SDK-TOOLS_CLONE\Package\Release\Azure.psd1```

4. After you build the commandlets project, you can then open a PowerShell window and you should be able to use the commandlets. Please note that if you want to rebuild the project, you have close the PowerShell window, and then reopen it.

# Quick start

1. Create an Azure hosted service called HelloWorld by typing

   ```New-AzureServiceProject HelloWorld```

2. Inside the HelloWorld folder, add a new Web Role by typing

   ```Add-AzureNodeWebRole``` or ```Add-AzurePHPWebRole```

3. Test out the application in the local emulator by typing

   ```Start-AzureEmulator -Launch```

4. You are now ready to publish to the cloud service. Go ahead and register for a Windows Azure account and make sure you have your credentials handy.

5. Get your account's publish settings and save them to a file by typing ```Get-AzurePublishSettingsFile```

6. Now import the settings

   ```Import-AzurePublishSettingsFile PATH_TO_PUBLISH_SETTINGS_FILE```

7. You are now ready to publish to the cloud. Make sure you specify a unique name for your application to ensure there aren't any conflicts during the publish process. Then type

   ```Publish-AzureServiceProject -ServiceName UNIQUE_NAME -Launch```

# Need Help?

Be sure to check out the [Windows Azure Developer Forums on Stack Overflow](http://go.microsoft.com/fwlink/?LinkId=234489) if you have trouble with the provided code.

# Contribute Code or Provide Feedback

If you would like to become an active contributor to this project please follow the instructions provided in [Windows Azure Projects Contribution Guidelines](http://windowsazure.github.com/guidelines.html).

If you encounter any bugs with the library please file an issue in the [Issues](https://github.com/WindowsAzure/azure-sdk-tools/issues) section of the project.

# Learn More

* [Windows Azure Developer Center](http://www.windowsazure.com/en-us/develop)
