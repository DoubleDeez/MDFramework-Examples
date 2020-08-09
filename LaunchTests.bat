godot -e -q --build-solutions
start cmd /k godot AutomaticTests/AutoTestsMain.tscn -mode=server
start cmd /k godot AutomaticTests/AutoTestsMain.tscn -mode=client
start cmd /k godot AutomaticTests/AutoTestsMain.tscn -mode=client
exit