extends Control

@onready var add_button = $VBoxContainer/VBoxContainer2/AddButton
@onready var container = $VBoxContainer/VBoxContainer2

var loading_station_scene := preload("res://ui/loadingStationEntry.tscn")
var entry_ids: Array = []  # ← you were missing this line

func _ready():
	add_button.pressed.connect(_on_add_button_pressed)

func _on_add_button_pressed():
	var entry = loading_station_scene.instantiate()

	var entry_id = "station_" + str(Time.get_ticks_usec())
	entry._setID(entry_id)
	entry_ids.append(entry_id)

	entry.delete_requested.connect(_on_delete_entry_requested.bind(entry, entry_id))

	container.add_child(entry)

	print("All current IDs: ", entry_ids)
	export_ids_to_json()

func _on_delete_entry_requested(entry, entry_id):
	container.remove_child(entry)
	entry.queue_free()
	entry_ids.erase(entry_id)

func export_ids_to_json():
	var data = { "station_ids": entry_ids }
	var json = JSON.stringify(data, "\t")
	print(json)
