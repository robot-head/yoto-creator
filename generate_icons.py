#!/usr/bin/env python3
"""
Generate all required icons for the Yoto Creator UWP application.
This script creates professional-looking icons with a modern design.
"""

from PIL import Image, ImageDraw, ImageFont
import os

def create_icon(size, filename, is_wide=False):
    """
    Create an icon with the Yoto Creator branding.

    Args:
        size: Tuple of (width, height)
        filename: Output filename
        is_wide: Whether this is a wide format icon
    """
    # Create image with transparent background
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Color scheme - vibrant and friendly colors for a children's content app
    primary_color = (52, 168, 83)  # Green (similar to Yoto's branding)
    secondary_color = (251, 188, 5)  # Yellow/Gold accent
    bg_color = (255, 255, 255, 255)  # White background

    # Draw white background with rounded corners
    draw.rectangle([0, 0, size[0], size[1]], fill=bg_color)

    # Calculate dimensions for the logo elements
    width, height = size
    center_x, center_y = width // 2, height // 2

    if is_wide:
        # Wide format - create horizontal layout
        # Draw a speaker/audio icon on the left
        icon_size = int(height * 0.6)
        icon_x = int(width * 0.25)

        # Draw speaker circle
        padding = int(icon_size * 0.15)
        draw.ellipse(
            [icon_x - icon_size//2 + padding, center_y - icon_size//2 + padding,
             icon_x + icon_size//2 - padding, center_y + icon_size//2 - padding],
            fill=primary_color
        )

        # Draw sound wave circles
        wave_color = secondary_color
        for i in range(3):
            wave_offset = int(icon_size * 0.15 * (i + 1))
            draw.arc(
                [icon_x + icon_size//4 - wave_offset, center_y - wave_offset,
                 icon_x + icon_size//4 + wave_offset * 2, center_y + wave_offset],
                start=-45, end=45,
                fill=wave_color, width=int(height * 0.05)
            )
    else:
        # Square format - create centered icon
        icon_size = int(min(width, height) * 0.7)

        # Draw main circle (speaker)
        padding = int(icon_size * 0.1)
        draw.ellipse(
            [center_x - icon_size//2 + padding, center_y - icon_size//2 + padding,
             center_x + icon_size//2 - padding, center_y + icon_size//2 - padding],
            fill=primary_color
        )

        # Draw inner circle
        inner_size = int(icon_size * 0.5)
        draw.ellipse(
            [center_x - inner_size//2, center_y - inner_size//2,
             center_x + inner_size//2, center_y + inner_size//2],
            fill=secondary_color
        )

        # Draw small center circle
        tiny_size = int(icon_size * 0.2)
        draw.ellipse(
            [center_x - tiny_size//2, center_y - tiny_size//2,
             center_x + tiny_size//2, center_y + tiny_size//2],
            fill=primary_color
        )

    # Save the icon
    output_path = os.path.join('/home/user/yoto-creator/YotoCreator/Assets', filename)
    img.save(output_path, 'PNG')
    print(f"✓ Created {filename} ({size[0]}x{size[1]})")

def create_splash_screen(size, filename):
    """
    Create a splash screen with the Yoto Creator branding and text.

    Args:
        size: Tuple of (width, height)
        filename: Output filename
    """
    # Create image
    img = Image.new('RGBA', size, (255, 255, 255, 255))
    draw = ImageDraw.Draw(img)

    width, height = size
    center_x, center_y = width // 2, height // 2

    # Colors
    primary_color = (52, 168, 83)
    secondary_color = (251, 188, 5)
    text_color = (66, 66, 66)

    # Draw large centered logo
    icon_size = int(height * 0.4)

    # Draw main circle
    draw.ellipse(
        [center_x - icon_size//2, center_y - icon_size//2 - int(height * 0.08),
         center_x + icon_size//2, center_y + icon_size//2 - int(height * 0.08)],
        fill=primary_color
    )

    # Draw inner circle
    inner_size = int(icon_size * 0.6)
    draw.ellipse(
        [center_x - inner_size//2, center_y - inner_size//2 - int(height * 0.08),
         center_x + inner_size//2, center_y + inner_size//2 - int(height * 0.08)],
        fill=secondary_color
    )

    # Draw center circle
    tiny_size = int(icon_size * 0.25)
    draw.ellipse(
        [center_x - tiny_size//2, center_y - tiny_size//2 - int(height * 0.08),
         center_x + tiny_size//2, center_y + tiny_size//2 - int(height * 0.08)],
        fill=primary_color
    )

    # Try to add text (will use default font if custom font not available)
    try:
        # Try to load a nice font
        font_size = int(height * 0.08)
        try:
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", font_size)
        except:
            font = ImageFont.load_default()

        text = "Yoto Creator"

        # Get text bounding box for centering
        bbox = draw.textbbox((0, 0), text, font=font)
        text_width = bbox[2] - bbox[0]
        text_height = bbox[3] - bbox[1]

        # Draw text below the logo
        text_y = center_y + icon_size//2 + int(height * 0.05)
        draw.text(
            (center_x - text_width//2, text_y),
            text,
            fill=text_color,
            font=font
        )
    except Exception as e:
        print(f"  Note: Could not add text to splash screen: {e}")

    # Save the splash screen
    output_path = os.path.join('/home/user/yoto-creator/YotoCreator/Assets', filename)
    img.save(output_path, 'PNG')
    print(f"✓ Created {filename} ({size[0]}x{size[1]})")

def main():
    """Generate all required icons for the UWP application."""
    print("Generating icons for Yoto Creator UWP application...\n")

    # Ensure Assets directory exists
    assets_dir = '/home/user/yoto-creator/YotoCreator/Assets'
    os.makedirs(assets_dir, exist_ok=True)

    # Generate all required icons
    # Note: Sizes are for scale-200 (200% DPI) as per UWP standards

    # 1. Store Logo (50x50 px actual = 100x100 at scale-200)
    create_icon((50, 50), 'StoreLogo.png')

    # 2. Square 44x44 Logo (actual 44x44 = 88x88 at scale-200)
    create_icon((44, 44), 'Square44x44Logo.png')

    # 3. Square 150x150 Logo (actual 150x150 = 300x300 at scale-200)
    create_icon((150, 150), 'Square150x150Logo.png')

    # 4. Wide 310x150 Logo (actual 310x150 = 620x300 at scale-200)
    create_icon((310, 150), 'Wide310x150Logo.png', is_wide=True)

    # 5. Splash Screen (actual 620x300 = 1240x600 at scale-200)
    create_splash_screen((620, 300), 'SplashScreen.png')

    # 6. Lock Screen Logo scale-200 (24x24 px actual = 48x48 at scale-200)
    create_icon((24, 24), 'LockScreenLogo.scale-200.png')

    # 7. Square 44x44 Logo targetsize-24 unplated (24x24 px)
    create_icon((24, 24), 'Square44x44Logo.targetsize-24_altform-unplated.png')

    print("\n✓ All icons generated successfully!")
    print(f"Icons saved to: {assets_dir}")

if __name__ == '__main__':
    main()
