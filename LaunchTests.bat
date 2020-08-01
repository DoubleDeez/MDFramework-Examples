godot -e -q --build-solutions
start cmd /k godot AutomaticTests/AutomaticTestsMain.tscn -mode=server
start cmd /k godot AutomaticTests/AutomaticTestsMain.tscn -mode=client
start cmd /k godot AutomaticTests/AutomaticTestsMain.tscn -mode=client
exit