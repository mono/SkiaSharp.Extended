using System.Numerics;
using SkiaSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Gif;

namespace MorphysicsImageGenerator;

public static class GifGeneratorImageSharp
{
    public static void GenerateParticlesGravityGif(string outputPath)
    {
        Console.WriteLine("🎬 Generating particles with gravity GIF...");
        
        const int width = 600;
        const int height = 800;
        const int fps = 20; // Lower FPS for smaller file size
        const int durationSeconds = 4;
        const int frameCount = fps * durationSeconds;
        const float dt = 1f / fps;
        
        using var gif = new Image<Rgba32>(width, height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0; // Loop forever
        
        // Initialize physics
        var particles = new List<ParticleState>();
        var random = new Random(42);
        var gravity = new Vector2(0, 300f);
        
        for (int frame = 0; frame < frameCount; frame++)
        {
            // Spawn new particles periodically
            if (frame % 3 == 0 && particles.Count < 60)
            {
                particles.Add(new ParticleState
                {
                    Position = new Vector2(width / 2 + (float)(random.NextDouble() - 0.5) * 100, 50),
                    Velocity = new Vector2((float)(random.NextDouble() - 0.5) * 50, -50),
                    Color = SKColors.DeepSkyBlue,
                    Radius = 6f
                });
            }
            
            // Update physics
            foreach (var particle in particles)
            {
                particle.Velocity += gravity * dt;
                particle.Position += particle.Velocity * dt;
                
                // Bounce off bottom
                if (particle.Position.Y > height - particle.Radius)
                {
                    particle.Position = new Vector2(particle.Position.X, height - particle.Radius);
                    particle.Velocity = new Vector2(particle.Velocity.X, -particle.Velocity.Y * 0.7f);
                }
                
                // Bounce off sides
                if (particle.Position.X < particle.Radius)
                {
                    particle.Position = new Vector2(particle.Radius, particle.Position.Y);
                    particle.Velocity = new Vector2(-particle.Velocity.X * 0.7f, particle.Velocity.Y);
                }
                else if (particle.Position.X > width - particle.Radius)
                {
                    particle.Position = new Vector2(width - particle.Radius, particle.Position.Y);
                    particle.Velocity = new Vector2(-particle.Velocity.X * 0.7f, particle.Velocity.Y);
                }
            }
            
            // Render frame using SkiaSharp
            var frameImage = RenderParticlesFrame(width, height, particles, $"Particles: {particles.Count}");
            
            // Add frame to GIF
            var frameMetadata = frameImage.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = 100 / fps; // Delay in 1/100th of a second
            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
        }
        
        // Remove the initial empty frame
        gif.Frames.RemoveFrame(0);
        
        // Save GIF
        gif.SaveAsGif(outputPath, new GifEncoder { ColorTableMode = GifColorTableMode.Local });
        
        Console.WriteLine($"  ✓ Created: {Path.GetFileName(outputPath)}");
    }
    
    public static void GenerateMorphingGif(string outputPath)
    {
        Console.WriteLine("🎬 Generating morphing animation GIF...");
        
        const int width = 600;
        const int height = 600;
        const int fps = 20;
        const int durationSeconds = 6;
        const int frameCount = fps * durationSeconds;
        
        using var gif = new Image<Rgba32>(width, height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;
        
        for (int frame = 0; frame < frameCount; frame++)
        {
            // Calculate morph progress
            float rawProgress = (float)frame / (frameCount / 2);
            float progress = rawProgress > 1f ? 2f - rawProgress : rawProgress;
            progress = Math.Clamp(progress, 0f, 1f);
            
            // Apply easing
            progress = progress < 0.5f
                ? 2f * progress * progress
                : 1f - (float)Math.Pow(-2f * progress + 2f, 2f) / 2f;
            
            // Render frame
            var frameImage = RenderMorphingFrame(width, height, progress);
            
            // Add to GIF
            var frameMetadata = frameImage.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = 100 / fps;
            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
        }
        
        gif.Frames.RemoveFrame(0);
        gif.SaveAsGif(outputPath, new GifEncoder { ColorTableMode = GifColorTableMode.Local });
        
        Console.WriteLine($"  ✓ Created: {Path.GetFileName(outputPath)}");
    }
    
    public static void GenerateAttractorGif(string outputPath)
    {
        Console.WriteLine("🎬 Generating attractor demonstration GIF...");
        
        const int width = 600;
        const int height = 800;
        const int fps = 20;
        const int durationSeconds = 6;
        const int frameCount = fps * durationSeconds;
        const float dt = 1f / fps;
        
        using var gif = new Image<Rgba32>(width, height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;
        
        // Initialize physics
        var particles = new List<ParticleState>();
        var random = new Random(42);
        var attractorPos = new Vector2(width / 2, height / 2);
        var attractorStrength = 10000f;
        
        for (int frame = 0; frame < frameCount; frame++)
        {
            // Spawn particles
            if (frame % 2 == 0 && particles.Count < 80)
            {
                particles.Add(new ParticleState
                {
                    Position = new Vector2((float)(random.NextDouble() * width), 50),
                    Velocity = new Vector2(0, 50),
                    Color = SKColors.Orange,
                    Radius = 5f
                });
            }
            
            // Update physics - FIXED: Inverse square law
            foreach (var particle in particles)
            {
                var toAttractor = attractorPos - particle.Position;
                var distanceSq = toAttractor.LengthSquared();
                if (distanceSq > 1f)
                {
                    var distance = (float)Math.Sqrt(distanceSq);
                    // Inverse square law: force = (direction / distance) * (strength / max(distance², minDist²))
                    var force = toAttractor / distance * (attractorStrength / Math.Max(distanceSq, 100f));
                    particle.Velocity += force * dt;
                }
                
                particle.Position += particle.Velocity * dt;
                particle.Velocity *= 0.99f; // Damping
            }
            
            particles.RemoveAll(p => Vector2.Distance(p.Position, attractorPos) < 30f);
            
            // Render frame
            var frameImage = RenderAttractorFrame(width, height, particles, new[] { attractorPos }, particles.Count);
            
            // Add to GIF
            var frameMetadata = frameImage.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = 100 / fps;
            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
        }
        
        gif.Frames.RemoveFrame(0);
        gif.SaveAsGif(outputPath, new GifEncoder { ColorTableMode = GifColorTableMode.Local });
        
        Console.WriteLine($"  ✓ Created: {Path.GetFileName(outputPath)}");
    }
    
    public static void GenerateMultiAttractorGif(string outputPath)
    {
        Console.WriteLine("🎬 Generating multi-attractor demonstration GIF...");
        
        const int width = 800;
        const int height = 600;
        const int fps = 20;
        const int durationSeconds = 8;
        const int frameCount = fps * durationSeconds;
        const float dt = 1f / fps;
        
        using var gif = new Image<Rgba32>(width, height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;
        
        // Initialize physics
        var particles = new List<ParticleState>();
        var random = new Random(42);
        
        // Three attractors in triangle formation
        var attractors = new[]
        {
            new Vector2(width / 4, height / 2),
            new Vector2(3 * width / 4, height / 2),
            new Vector2(width / 2, height / 4)
        };
        var attractorStrength = 8000f;
        
        for (int frame = 0; frame < frameCount; frame++)
        {
            // Spawn particles from top
            if (frame % 2 == 0 && particles.Count < 100)
            {
                particles.Add(new ParticleState
                {
                    Position = new Vector2((float)(random.NextDouble() * width), height - 50),
                    Velocity = new Vector2((float)(random.NextDouble() - 0.5) * 30, -30),
                    Color = SKColors.DeepSkyBlue,
                    Radius = 4f
                });
            }
            
            // Update physics with multiple attractors
            foreach (var particle in particles)
            {
                foreach (var attractorPos in attractors)
                {
                    var toAttractor = attractorPos - particle.Position;
                    var distanceSq = toAttractor.LengthSquared();
                    if (distanceSq > 1f)
                    {
                        var distance = (float)Math.Sqrt(distanceSq);
                        var force = toAttractor / distance * (attractorStrength / Math.Max(distanceSq, 100f));
                        particle.Velocity += force * dt;
                    }
                }
                
                particle.Position += particle.Velocity * dt;
                particle.Velocity *= 0.98f; // Less damping for more dynamic motion
            }
            
            // Remove particles that reached any attractor
            particles.RemoveAll(p => attractors.Any(a => Vector2.Distance(p.Position, a) < 25f));
            
            // Render frame
            var frameImage = RenderAttractorFrame(width, height, particles, attractors, particles.Count);
            
            // Add to GIF
            var frameMetadata = frameImage.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = 100 / fps;
            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
        }
        
        gif.Frames.RemoveFrame(0);
        gif.SaveAsGif(outputPath, new GifEncoder { ColorTableMode = GifColorTableMode.Local });
        
        Console.WriteLine($"  ✓ Created: {Path.GetFileName(outputPath)}");
    }
    
    public static void GenerateStickyZoneGif(string outputPath)
    {
        Console.WriteLine("🎬 Generating sticky zone demonstration GIF...");
        
        const int width = 700;
        const int height = 700;
        const int fps = 20;
        const int durationSeconds = 10;
        const int frameCount = fps * durationSeconds;
        const float dt = 1f / fps;
        
        using var gif = new Image<Rgba32>(width, height);
        var gifMetadata = gif.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;
        
        // Initialize physics
        var particles = new List<ParticleState>();
        var stuckParticles = new List<ParticleState>();
        var random = new Random(42);
        var gravity = new Vector2(0, 150f);
        
        // Sticky zones (circles)
        var stickyZones = new[]
        {
            new StickyZone { Position = new Vector2(width / 3, height / 2), Radius = 80f, Color = new SKColor(50, 200, 50, 60) },
            new StickyZone { Position = new Vector2(2 * width / 3, height / 2), Radius = 80f, Color = new SKColor(200, 50, 50, 60) }
        };
        
        for (int frame = 0; frame < frameCount; frame++)
        {
            // Spawn particles from top
            if (frame % 3 == 0 && particles.Count + stuckParticles.Count < 120)
            {
                particles.Add(new ParticleState
                {
                    Position = new Vector2((float)(random.NextDouble() * width), 20),
                    Velocity = new Vector2((float)(random.NextDouble() - 0.5) * 50, 0),
                    Color = SKColors.Purple,
                    Radius = 4f
                });
            }
            
            // Update physics
            var particlesToStick = new List<(ParticleState particle, int zoneIndex)>();
            
            foreach (var particle in particles)
            {
                particle.Velocity += gravity * dt;
                particle.Position += particle.Velocity * dt;
                
                // Bounce off bottom
                if (particle.Position.Y > height - particle.Radius)
                {
                    particle.Position = new Vector2(particle.Position.X, height - particle.Radius);
                    particle.Velocity = new Vector2(particle.Velocity.X, -particle.Velocity.Y * 0.6f);
                }
                
                // Bounce off sides
                if (particle.Position.X < particle.Radius)
                {
                    particle.Position = new Vector2(particle.Radius, particle.Position.Y);
                    particle.Velocity = new Vector2(-particle.Velocity.X * 0.6f, particle.Velocity.Y);
                }
                else if (particle.Position.X > width - particle.Radius)
                {
                    particle.Position = new Vector2(width - particle.Radius, particle.Position.Y);
                    particle.Velocity = new Vector2(-particle.Velocity.X * 0.6f, particle.Velocity.Y);
                }
                
                // Check sticky zones
                for (int i = 0; i < stickyZones.Length; i++)
                {
                    var zone = stickyZones[i];
                    var distance = Vector2.Distance(particle.Position, zone.Position);
                    if (distance < zone.Radius)
                    {
                        // 30% chance to stick when inside zone
                        if (random.NextDouble() < 0.3)
                        {
                            particlesToStick.Add((particle, i));
                            break;
                        }
                    }
                }
            }
            
            // Move stuck particles to stuck list
            foreach (var (particle, zoneIndex) in particlesToStick)
            {
                particles.Remove(particle);
                particle.Velocity = Vector2.Zero;
                particle.Color = zoneIndex == 0 ? SKColors.Green : SKColors.Red;
                stuckParticles.Add(particle);
            }
            
            // Render frame
            var frameImage = RenderStickyZoneFrame(width, height, particles, stuckParticles, stickyZones);
            
            // Add to GIF
            var frameMetadata = frameImage.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = 100 / fps;
            gif.Frames.AddFrame(frameImage.Frames.RootFrame);
        }
        
        gif.Frames.RemoveFrame(0);
        gif.SaveAsGif(outputPath, new GifEncoder { ColorTableMode = GifColorTableMode.Local });
        
        Console.WriteLine($"  ✓ Created: {Path.GetFileName(outputPath)}");
    }
    
    private static Image<Rgba32> RenderParticlesFrame(int width, int height, List<ParticleState> particles, string label)
    {
        // Render using SkiaSharp
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(250, 250, 250));
        
        // Draw particles
        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        foreach (var particle in particles)
        {
            paint.Color = particle.Color;
            canvas.DrawCircle(particle.Position.X, particle.Position.Y, particle.Radius, paint);
        }
        
        // Draw label
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 20,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
        };
        canvas.DrawText(label, 20, 30, textPaint);
        
        // Convert to ImageSharp
        return SkiaToImageSharp(surface, width, height);
    }
    
    private static Image<Rgba32> RenderMorphingFrame(int width, int height, float progress)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        
        // Draw morphed shape
        using var paint = new SKPaint
        {
            Color = SKColors.DeepPink,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        
        using var strokePaint = new SKPaint
        {
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        };
        
        var size = 200f;
        var cornerRadius = progress * size / 2;
        var rect = new SKRect(width / 2 - size / 2, height / 2 - size / 2,
                             width / 2 + size / 2, height / 2 + size / 2);
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);
        canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, strokePaint);
        
        // Draw labels
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 24,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
            TextAlign = SKTextAlign.Center
        };
        
        var label = progress < 0.01f ? "Square" : progress > 0.99f ? "Circle" : $"{progress:P0}";
        canvas.DrawText(label, width / 2, height - 50, textPaint);
        canvas.DrawText("Square → Circle Morph", width / 2, 40, textPaint);
        
        return SkiaToImageSharp(surface, width, height);
    }
    
    private static Image<Rgba32> RenderAttractorFrame(int width, int height, List<ParticleState> particles, Vector2[] attractorPositions, int count)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(245, 250, 255));
        
        // Draw attractors
        using var attractorPaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        };
        using var centerPaint = new SKPaint { Color = SKColors.Red, IsAntialias = true };
        
        foreach (var attractorPos in attractorPositions)
        {
            canvas.DrawCircle(attractorPos.X, attractorPos.Y, 40, attractorPaint);
            canvas.DrawCircle(attractorPos.X, attractorPos.Y, 8, centerPaint);
        }
        
        // Draw particles
        using var particlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        foreach (var particle in particles)
        {
            particlePaint.Color = particle.Color;
            canvas.DrawCircle(particle.Position.X, particle.Position.Y, particle.Radius, particlePaint);
        }
        
        // Draw labels
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 24,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
            TextAlign = SKTextAlign.Center
        };
        var title = attractorPositions.Length > 1 ? $"{attractorPositions.Length} Attractors Demo" : "Attractor Demo";
        canvas.DrawText(title, width / 2, 40, textPaint);
        canvas.DrawText($"Particles: {count}", width / 2, height - 30, textPaint);
        
        return SkiaToImageSharp(surface, width, height);
    }
    
    private static Image<Rgba32> RenderStickyZoneFrame(int width, int height, List<ParticleState> particles, List<ParticleState> stuckParticles, StickyZone[] stickyZones)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(250, 250, 250));
        
        // Draw sticky zones (semi-transparent circles)
        using var zonePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var zoneBorderPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
        
        foreach (var zone in stickyZones)
        {
            zonePaint.Color = zone.Color;
            zoneBorderPaint.Color = new SKColor(zone.Color.Red, zone.Color.Green, zone.Color.Blue, 150);
            canvas.DrawCircle(zone.Position.X, zone.Position.Y, zone.Radius, zonePaint);
            canvas.DrawCircle(zone.Position.X, zone.Position.Y, zone.Radius, zoneBorderPaint);
        }
        
        // Draw free particles
        using var particlePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        foreach (var particle in particles)
        {
            particlePaint.Color = particle.Color;
            canvas.DrawCircle(particle.Position.X, particle.Position.Y, particle.Radius, particlePaint);
        }
        
        // Draw stuck particles
        foreach (var particle in stuckParticles)
        {
            particlePaint.Color = particle.Color;
            canvas.DrawCircle(particle.Position.X, particle.Position.Y, particle.Radius, particlePaint);
        }
        
        // Draw labels
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 24,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("Sticky Zones Demo", width / 2, 40, textPaint);
        canvas.DrawText($"Free: {particles.Count} | Stuck: {stuckParticles.Count}", width / 2, height - 30, textPaint);
        
        return SkiaToImageSharp(surface, width, height);
    }
    
    private static Image<Rgba32> SkiaToImageSharp(SKSurface surface, int width, int height)
    {
        // Get pixel data from SkiaSharp
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var bytes = data.ToArray();
        
        // Load into ImageSharp
        return SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
    }
    
    private class ParticleState
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public SKColor Color { get; set; }
        public float Radius { get; set; }
    }
    
    private class StickyZone
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public SKColor Color { get; set; }
    }
}
