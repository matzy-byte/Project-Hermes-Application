extends HBoxContainer

signal delete_requested

@onready var delete_button = $Button
var entry_id: String = ""

func _ready():
	delete_button.pressed.connect(_on_delete_button_pressed)

func _on_delete_button_pressed():
	emit_signal("delete_requested")

func _setID(id: String):
	entry_id = id
