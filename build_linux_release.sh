godot_path='./vendor_godot/Godot_v4.2.1-stable_mono_win64.exe'

# will ignore dotfiles like .gitkeep
rm -rf ./build/linux_release/*
$godot_path ./source/project.godot --headless --export-release "Linux/X11 x64" ../build/linux_release/GunGame.x86_64
