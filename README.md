# SciChart.Avalonia.XPF.Examples
This repository contains multi-platform (Windows and Linux) SciChart Examples Demo for [Avalonia XPF](https://avaloniaui.net/XPF) framework. Please find out more about SciChart for Avalonia XPF in [this dedicated blog article](https://www.scichart.com/blog/running-scichart-wpf-on-linux-its-possible-heres-how/).
SciChart Examples Demo requires a valid Avalonia XPF license key to run. Please find environment setup and licensing instructions below.

# Configuring .NET packages
## Windows OS
### Installing .NET packages
Open [.NET packages download](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and install the .NET 6.0 SDK (v6.0.24 or later)

## Ubuntu OS (Linux)
### Removing Ubuntu .NET packages
Many distributions provide a version of .NET in their package repositories, but these should not be used as they 
do not ship the required Microsoft.NET.Sdk.WindowsDesktop SDK. If your version of Ubuntu (Debian) provides 
.NET packages, you will need to de-prioritize the Ubuntu packages and use the Microsoft repository.

Remove the existing .NET packages from your distribution. You want to start over and ensure that you do not 
install them from the wrong repository.
> sudo apt remove 'dotnet*' 'aspnet*' 'netstandard*'

Create a file /etc/apt/preferences, if it doesn't already exist.
> sudo touch /etc/apt/preferences

Open the file in a text editor and add the following content:
> Package: *  
> Pin: origin "packages.microsoft.com"  
> Pin-Priority: 1001

### Installing Microsoft .NET packages
Installing with APT can be done with a few commands. Before you install .NET, open a terminal and run the following commands to 
add the Microsoft package signing key to your list of trusted keys and add the package repository.
> wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O pkg-ms-prod.deb  
> sudo dpkg -i pkg-ms-prod.deb  
> rm pkg-ms-prod.deb  
> sudo apt update  

After you have registered the Microsoft package repository, you can install .NET through the package manager 
with the apt install <package-name> command. 
To install .NET 6.0 SDK, use the following command:
> sudo apt install dotnet-sdk-6.0 -y

### Installing SciChart Dependencies
Open the Terminal and run the following commands.
Updating apt database (optional):
> sudo apt update

Installing PKG configuration:
> sudo apt install pkg-config -y

Installing build tools:
> sudo apt install build-essential -y

Installing OpenGL and GLEW:
> sudo apt install libgl1-mesa-dev libglew-dev -y

Installing SDL2:
> sudo apt install libsdl2-2.0-0 -y

Installing FreeType2:
> sudo apt install libfreetype-dev -y

Installing Sodium
> sudo apt install libsodium-dev -y

### Installing Avalonia XPF Dependencies
Run the following command to install Avalonia XPF dependencies: libice6, libsm6, libfontconfig1, libgdiplus.
> sudo apt install libice6 libsm6 libfontconfig1 libgdiplus -y

# Installing IDE
## Windows OS
It is possible to use either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Sudio Code](https://code.visualstudio.com/) on Windows. They can be downloaded at corresponding links.

## Ubuntu OS (Linux)
One of the IDE options available on Linux is [Visual Sudio Code](https://code.visualstudio.com/). To install it, open the Terminal and run the following command:
> sudo apt install code

Alternatively, you can install the VSCode using the Ubuntu Software app.

## Installing Visual Studio Code Extensions
Launch the VSCode, open the Extensions tab and install these extensions:
- C# Dev Kit
- IntelliCode for C# Dev Kit

# Opening and Running the Examples Demo
Download or clone the GitHub repository with SciChart Demo for Avalonia XPF.
Running the Examples Demo in **Visual Studio IDE** is pretty straightforward. Open Visual Studio, then go to "File -> Open -> Project/Solution" menu and select **SciChart.Demo.Avalonia.XPF.sln** file from this repo. Make sure **SciChart.Examples.Demo.Xpf** project is selected as the startup project.
To run the Examples Demo in **Visual Studio Code IDE**, open the IDE then go to "File -> Open Folder" menu and choose the folder with Examples Demo. Then, select **SciChart.Demo.Avalonia.XPF.sln** in Solution Explorer. 

### Licensing Avalonia XPF
To license SciChart Avalonia XPF, open **SciChart.Examples.Demo.Xpf.csproj** file. Add your Avalonia XPF license to it as follows:
```
<ItemGroup>
	RuntimeHostConfigurationOption Include="AvaloniaUI.Xpf.LicenseKey" Value="YOUR_LICENSE_KEY" />
</ItemGroup>
```
Besides, you need to add your Avalonia XPF License to **NuGet.config** file:
```
<packageSourceCredentials>
	<avalonia-xpf>
		<add key="Username" value="YOUR_USERNAME"/>
		<add key="ClearTextPassword" value="YOUR_LICENSE_KEY"/>
	</avalonia-xpf>
</packageSourceCredentials>
```
You could learn more about Avalonia XPF Licensing [at this link](https://avaloniaui.net/Blog/introducing-avalonia-xpf-trials-and-the-startup-license).

At this point you should be able to build and run the Examples Demo. If you experience any issues or want to leave your feedback, feel free to [contact us](https://www.scichart.com/contact-us/).

### Licensing SciChart
This Examples Demo app doesn't require SciChart license to run. However, creating your own application with SciChart for Avalonia XPF requires either an activated [SciChart Bundle](https://www.scichart.com/shop/) license or a running trial period for the [SciChart WPF](https://www.scichart.com/shop/) license. 
Head over to https://www.scichart.com/getting-started-scichart-wpf/ to learn how to activate a SciChart license or start a free 30-days trial. 
The multi-platform Licensing Wizard can be found on the [Downloads page](https://www.scichart.com/downloads/).
