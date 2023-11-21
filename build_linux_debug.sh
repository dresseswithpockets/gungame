godot_path='./vendor_godot/Godot_v4.1.3-stable_mono_win64.exe'

# will ignore dotfiles like .gitkeep
rm -rf ./build/linux_debug/*
$godot_path ./source/project.godot --headless --export-debug "Linux/X11 x64" ../build/linux_debug/GunGame.x86_64
