import os
import math
import argparse
from PIL import Image

def create_spritesheet(input_folder, output_folder):
	"""
	Processes gameObject folders in the input folder and creates spritesheets for each animation.

	:param input_folder: Path to the folder containing gameObject folders.
	:param output_folder: Path to the folder where spritesheets will be saved.
	"""
	# Process each gameObject folder in the input folder
	for game_object in sorted(os.listdir(input_folder)):
		game_object_path = os.path.join(input_folder, game_object)
		if not os.path.isdir(game_object_path):
			continue

		print(f"Processing gameObject: {game_object}")

		# Create a corresponding folder in the output directory
		game_object_output_path = os.path.join(output_folder, game_object)
		os.makedirs(game_object_output_path, exist_ok=True)

		# Process each animation folder inside the gameObject folder
		for animation_folder in sorted(os.listdir(game_object_path)):
			animation_folder_path = os.path.join(game_object_path, animation_folder)
			if not os.path.isdir(animation_folder_path):
				continue

			print(f"  Processing animation: {animation_folder}")

			# Generate the spritesheet for this animation
			generate_spritesheet_for_animation(
				animation_folder_path,
				os.path.join(game_object_output_path, f"{animation_folder}.png")
			)

def generate_spritesheet_for_animation(animation_folder, output_file):
	"""
	Creates a spritesheet from the PNG files in an animation folder.

	:param animation_folder: Path to the folder containing PNG files for the animation.
	:param output_file: Path to save the generated spritesheet.
	"""
	# Get all PNG files in the folder, sorted by the number in their name
	image_files = [f for f in os.listdir(animation_folder) if f.endswith('.png')]
	image_files.sort(key=lambda x: int(''.join(filter(str.isdigit, x))))  # Sort by numeric part

	if not image_files:
		print(f"    No PNG files found in {animation_folder}")
		return

	# Determine sprite dimensions from the first image
	first_image_path = os.path.join(animation_folder, image_files[0])
	first_image = Image.open(first_image_path)
	sprite_width, sprite_height = first_image.size

	print(f"    Sprite dimensions: {sprite_width}x{sprite_height}")

	# Calculate grid size for a square spritesheet
	n = len(image_files)
	grid_size = math.ceil(math.sqrt(n))  # Number of rows/columns in the square

	# Create a blank spritesheet
	sheet_width = grid_size * sprite_width
	sheet_height = grid_size * sprite_height
	spritesheet = Image.new("RGBA", (sheet_width, sheet_height), (0, 0, 0, 0))

	# Paste each sprite onto the spritesheet
	for index, file in enumerate(image_files):
		img_path = os.path.join(animation_folder, file)
		img = Image.open(img_path).convert("RGBA")

		x = (index % grid_size) * sprite_width
		y = (index // grid_size) * sprite_height
		spritesheet.paste(img, (x, y))

	# Trim any blank rows at the bottom of the spritesheet
	trimmed_height = calculate_trimmed_height(spritesheet, sprite_height, grid_size)
	if trimmed_height < sheet_height:
		print(f"    Trimming blank rows: New height = {trimmed_height}")
		spritesheet = spritesheet.crop((0, 0, sheet_width, trimmed_height))

	# Save the spritesheet
	spritesheet.save(output_file)
	print(f"    Spritesheet saved: {output_file}")

def calculate_trimmed_height(spritesheet, sprite_height, grid_size):
	"""
	Calculate the height of the spritesheet without fully blank rows at the bottom.

	:param spritesheet: The spritesheet image
	:param sprite_height: Height of each sprite
	:param grid_size: Number of rows/columns in the grid
	:return: New height of the spritesheet
	"""
	width, height = spritesheet.size
	for row in range(grid_size - 1, -1, -1):  # Start from the last row
		y_start = row * sprite_height
		y_end = y_start + sprite_height

		# Crop the row and check if it's completely blank
		row_image = spritesheet.crop((0, y_start, width, y_end))
		if not is_image_blank(row_image):
			return y_end  # Return the height up to this non-blank row
	return 0  # All rows are blank

def is_image_blank(image):
	"""
	Check if an image is completely blank (all pixels are transparent).

	:param image: A PIL image
	:return: True if the image is blank, False otherwise
	"""
	return all(pixel[3] == 0 for pixel in image.getdata())  # Check alpha channel

if __name__ == "__main__":
	parser = argparse.ArgumentParser(description="Create spritesheets for animations organized in gameObject folders.")
	parser.add_argument("input_folder", help="Path to the folder containing gameObject folders.")
	parser.add_argument("--output_folder", default="spritesheets", help="Path to the output folder for spritesheets.")

	args = parser.parse_args()

	create_spritesheet(
		input_folder=args.input_folder,
		output_folder=args.output_folder
	)
