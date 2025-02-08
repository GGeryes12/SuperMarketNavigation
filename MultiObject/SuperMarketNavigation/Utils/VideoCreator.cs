using System.Diagnostics;

namespace SuperMarketNavigation.Utils
{
    public static class VideoCreator
    {
        public static void CreateVideoFromImages(string ffmpegPath, string imagesFolder, string outputVideo, int framerate = 1)
        {
            // FFmpeg command arguments
            string arguments = $"-framerate {framerate} -i \"{imagesFolder}\\population_spread_gen_%d.png\" -vf \"pad=ceil(iw/2)*2:ceil(ih/2)*2\" -c:v libx264 -pix_fmt yuv420p \"{outputVideo}\"";

            // Start FFmpeg process
            Process ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                ffmpegProcess.Start();

                // Capture FFmpeg output (optional for debugging)
                string output = ffmpegProcess.StandardOutput.ReadToEnd();
                string error = ffmpegProcess.StandardError.ReadToEnd();

                ffmpegProcess.WaitForExit();

                if (ffmpegProcess.ExitCode == 0)
                {
                    Console.WriteLine("Video created successfully!");
                }
                else
                {
                    Console.WriteLine($"FFmpeg process failed with error: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running FFmpeg: {ex.Message}");
            }
        }
    }
}
