godot_path='./vendor_godot/Godot_v4.1.3-stable_mono_win64.exe'

# will ignore dotfiles like .gitkeep
rm -rf ./build/win64_release/*
$godot_path ./source/project.godot --headless --export-release "Windows x64" ../build/win64_release/GunGame.exe
