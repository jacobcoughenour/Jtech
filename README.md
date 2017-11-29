# JTech
An industrial automation overhaul for the game [Rust](https://playrust.com) using [Oxide](https://github.com/OxideMod/Oxide).

## Environment Setup
Visual Studio 2017 is recommended
You can get the Community Edition for free [here](https://www.visualstudio.com/downloads/)

You need a Rust dedicated server with the latested version of Oxide installed.
[Setting up a rust server](http://oxidemod.org/threads/setting-up-a-rust-server-with-windows.5743/)

Once you have the server setup, you need to create an *_Environment Variable_* on your system called **RustServerDir** with the path to your server rust_dedicated folder as the value (Ex: C:\steamcmd\steamapps\common\rust_dedicated).  The system variable is used by the .csproject file for assembly references and the build output.
[How to set an Environment Variable](https://superuser.com/a/284351)

Now you can open JTech.sln in Visual Studio and get started!

## Build

To build and run JTech on your server, just build the JTech project (Ctrl-Shift-B).  This starts the PluginMerger script that combines all the .cs files to JTech.cs and saves it to the build folder and your server plugins folder.  If you have the your server running, oxide auto reloads the plugin for you.