extends SceneTree

const BOARDROOM_SCENE := preload("res://scenes/boardroom/boardroom.tscn")


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output_path := "/tmp/featherfall-godot-smoke.png"
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--output="):
			output_path = argument.trim_prefix("--output=")

	root.add_child(BOARDROOM_SCENE.instantiate())
	for _frame in range(12):
		await process_frame
		await physics_frame
	await RenderingServer.frame_post_draw

	var image := root.get_texture().get_image()
	var result := image.save_png(output_path)
	if result != OK:
		printerr("Unable to save smoke capture: %s" % error_string(result))
		quit(1)
		return
	print("CAPTURE: %s (%dx%d)" % [output_path, image.get_width(), image.get_height()])
	quit(0)
