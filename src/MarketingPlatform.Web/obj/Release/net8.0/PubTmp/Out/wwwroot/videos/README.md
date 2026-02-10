# Product Demo Video

This directory should contain the product demonstration video for the landing page hero section.

## Required File

**Filename:** `product-demo.mp4`

## Video Specifications

### Format & Codec
- **Container:** MP4 (H.264)
- **Video Codec:** H.264 (AVC)
- **Audio Codec:** AAC
- **Compatibility:** HTML5 video element

### Dimensions
- **Resolution:** 1280x720px (720p) or 1920x1080px (1080p)
- **Aspect Ratio:** 16:9
- **Frame Rate:** 30 fps

### File Size
- **Recommended:** 5-15 MB
- **Maximum:** 25 MB
- **Optimization:** Use web-optimized compression

### Duration
- **Recommended:** 30-90 seconds
- **Maximum:** 2 minutes

## Content Recommendations

The demo video should showcase:
1. **Dashboard Overview** (5-10s) - Show the main interface
2. **Campaign Creation** (10-15s) - Creating a new SMS/Email campaign
3. **Template Selection** (5-10s) - Choosing from template library
4. **Analytics View** (10-15s) - Real-time campaign analytics
5. **Multi-Channel Features** (10-15s) - SMS, MMS, Email capabilities
6. **Results/Success** (5-10s) - Show successful campaign metrics

## Video Creation Tools

### Professional Tools
- **Adobe Premiere Pro** - Industry standard video editing
- **Final Cut Pro** - Mac video editing
- **DaVinci Resolve** - Free professional editing software

### Screen Recording Tools
- **OBS Studio** - Free, open-source screen recorder
- **Loom** - Quick screen recording with annotations
- **Camtasia** - Screen recording + editing
- **ScreenFlow** - Mac screen recording

### Optimization Tools
- **HandBrake** - Free video compression/conversion
- **FFmpeg** - Command-line video processing
- **CloudConvert** - Online video conversion

## Fallback Support

The landing page includes two fallback mechanisms:

1. **GIF Fallback** (`/images/product-demo.gif`)
   - Shown if video fails to load
   - Should be a shorter, optimized version (3-5 MB max)

2. **Poster Image** (`/images/video-poster.jpg`)
   - Thumbnail shown before video plays
   - 1280x720px JPG image
   - Should be an attractive frame from the video

## Compression Example (FFmpeg)

```bash
# Compress video to web-optimized MP4
ffmpeg -i input.mp4 -vcodec h264 -acodec aac -b:v 2M -b:a 128k -s 1280x720 product-demo.mp4

# Create poster image from video frame
ffmpeg -i product-demo.mp4 -ss 00:00:02 -vframes 1 ../images/video-poster.jpg

# Create GIF from video (optional fallback)
ffmpeg -i product-demo.mp4 -vf "fps=10,scale=640:-1:flags=lanczos" -t 10 ../images/product-demo.gif
```

## Current Status

⚠️ **Video file missing** - Upload `product-demo.mp4` to this directory
✅ **Video player ready** - HTML5 video element configured with controls
✅ **Fallback support** - GIF and poster image fallbacks enabled

## Alternative: Use Placeholder

If you don't have a demo video yet, you can:
1. Use a static image in the hero section instead
2. Create a simple slideshow of screenshots
3. Use an animated GIF until video is ready
4. Record a quick demo using Loom or similar tool

## Testing

After uploading the video, test:
1. ✅ Video plays in Chrome, Firefox, Safari, Edge
2. ✅ Video poster image displays before play
3. ✅ Play button overlay appears and functions
4. ✅ Video controls work (play, pause, volume, fullscreen)
5. ✅ Video is responsive on mobile devices
6. ✅ File size allows fast loading (< 5 seconds on average connection)
