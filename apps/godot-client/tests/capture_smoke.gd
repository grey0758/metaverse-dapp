extends SceneTree

const BOARDROOM_SCENE := preload("res://scenes/boardroom/boardroom.tscn")


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output_path := "/tmp/featherfall-godot-smoke.png"
	var requested_seat: StringName = &""
	var requested_position := Vector2.INF
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--output="):
			output_path = argument.trim_prefix("--output=")
		elif argument.begins_with("--seat="):
			requested_seat = StringName(argument.trim_prefix("--seat="))
		elif argument.begins_with("--position="):
			var parts := argument.trim_prefix("--position=").split(",", false, 1)
			if parts.size() == 2 and parts[0].is_valid_float() and parts[1].is_valid_float():
				requested_position = Vector2(parts[0].to_float(), parts[1].to_float())

	var boardroom := BOARDROOM_SCENE.instantiate() as Boardroom
	root.add_child(boardroom)
	for _frame in range(12):
		await process_frame
		await physics_frame
	if requested_seat != &"":
		var seat := BoardroomLayout.seat_by_id(requested_seat)
		if seat.is_empty():
			printerr("Unknown smoke-capture seat: %s" % requested_seat)
			quit(1)
			return
		boardroom.player.global_position = seat.approach
		boardroom._update_interaction()
		boardroom._interact()
		for _frame in range(36):
			await process_frame
			await physics_frame
	elif requested_position != Vector2.INF:
		boardroom.player.global_position = requested_position
		for _frame in range(24):
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
