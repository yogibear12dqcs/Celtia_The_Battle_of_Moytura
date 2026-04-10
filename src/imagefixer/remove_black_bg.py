#!/usr/bin/env python3
from PIL import Image
import os

# Input and output directories
input_dir = "CeltiaGame/wwwroot/assets/bobs"
output_dir = "CeltiaGame/wwwroot/assets/bobs_new"

# Create output directory if it doesn't exist
os.makedirs(output_dir, exist_ok=True)

# Process each image
for filename in os.listdir(input_dir):
    if filename.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp')):
        input_path = os.path.join(input_dir, filename)
        
        # Change extension to .png for transparency support
        output_filename = os.path.splitext(filename)[0] + '.png'
        output_path = os.path.join(output_dir, output_filename)
        
        print(f"Processing {filename}...")
        
        # Open the image
        img = Image.open(input_path)
        
        # Convert to RGBA if not already
        img = img.convert("RGBA")
        
        # Get the image data
        datas = img.getdata()
        
        # Create new image data with black pixels made transparent
        new_data = []
        threshold = 30  # Tolerance for near-black colors
        
        for item in datas:
            # Check if pixel is black or near-black
            if item[0] < threshold and item[1] < threshold and item[2] < threshold:
                # Make it transparent
                new_data.append((255, 255, 255, 0))
            else:
                # Keep original
                new_data.append(item)
        
        # Update image data
        img.putdata(new_data)
        
        # Save as PNG with transparency
        img.save(output_path, "PNG")
        print(f"Saved to {output_path}")

print("\nDone! All images processed.")
