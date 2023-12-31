# GunGame

### Cloning This Repository

If you are new to Git (the command line tool), follow these instructions to get started:
1. [Create a GitHub account](https://docs.github.com/en/get-started/quickstart/creating-an-account-on-github)
1. Follow the [official GitHub instructions to set up Git locally](https://docs.github.com/en/get-started/quickstart/set-up-git)
1. Follow the [official instructions to clone this repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository?tool=webui)

## Getting Started

You must follow these instructions before attempting to run or build the project, and before attempting to create or edit maps in Trenchbroom.

If you're on Linux, these instructions should work about the same, you'll just need to download/install the linux releases of each of the following dependencies.

If you're on MacOS... good luck (:

### First Time Setup
1. Download and Install the [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) SDK
1. Download Godot 4.2.1 (Mono/.NET Build) for [Windows](https://github.com/godotengine/godot/releases/download/4.2.1-stable/Godot_v4.2.1-stable_mono_win64.zip), and extract into `vendor_godot/`
1. Download Trenchbroom Nightly for [Windows](https://github.com/TrenchBroom/TrenchBroom/suites/18307704645/artifacts/1059032729), and extract into `vendor_trenchbroom/`
1. Open the project in Godot through one of these means:
    - run the Godot executable in `vendor_godot`, then select the `source/project.godot` project file.
    - run `open_project.bat` in the root of this repository
1. Build dependencies by pressing `Alt-B`, or by clicking "Build" in the top right of the editor
1. Close & reopen the project in Godot

After setting up, you should have a folder structure that looks roughly like this:
![Desired folder structure](readme_assets/folder_structure.png)

### Trenchbroom Game Configuration

Before using Trenchbroom, you must configure it with our game's info, entities, asset types, and texture location.

1. Open the project in Godot
1. Under the FileSystem panel, double click on the `qodot_trenchbroom_config_folder.tres` asset
    - the Inspector panel will now be inspecting this asset
1. In the Inspector panel, ensure that the "Trenchbroom Games Folder" property is set to `../vendor_trenchbroom/games`
1. Click the "Export File" check box in the Inspector Panel
    - it will automatically uncheck immediately after you click it
    - this button exports all trenchbroom configurations for this project to `../vendor_trenchbroom/games/GunGame/`
1. Open trenchbroom by running `vendor_trenchbroom/TrenchBroom.exe`
1. Open preferences in trenchbroom
    - from the "Welcome to TrenchBroom" dialogue, you can get there by clicking "New map..." -> "Open preferences...".
1. Update the GunGame Game Path to point to the *absolute path* to `gungame/source/trenchbroom`.
    - e.g. `C:\Users\DressesDigital\source\repos\gungame\source\trenchbroom`
1. Click "OK"

You should be able to select "GunGame" when creating a new map now. You can also edit existing maps in the project.

### Creating a New Map

WIP

## License

The source assets authored by Dresses Digital members is [licensed under the Apache License 2.0](LICENSE):

```
   Copyright 2023 Dresses Digital

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
```

Some assets distributed may be licensed differently. See [licenses](licenses) for licenses of other libraries and tools used.
