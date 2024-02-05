godot_path='./vendor_godot/Godot_v4.2.1-stable_mono_win64.exe'

# will ignore dotfiles like .gitkeep
rm -rf ./build/win64_debug/*
$godot_path ./source/project.godot --headless --export-debug "Windows x64" ../build/win64_debug/GunGame.exe
