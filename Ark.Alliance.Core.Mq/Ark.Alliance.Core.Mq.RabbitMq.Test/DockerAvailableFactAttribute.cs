using Xunit;

internal sealed class DockerAvailableFactAttribute : FactAttribute
{
    private static bool IsDockerAvailable()
    {
        if (OperatingSystem.IsWindows())
        {
            var pipe = @"\\.\pipe\docker_engine";
            return File.Exists(pipe);
        }
        else
        {
            return File.Exists("/var/run/docker.sock");
        }
    }

    public DockerAvailableFactAttribute()
    {
        if (!IsDockerAvailable())
        {
            Skip = "Docker engine not available";
        }
    }
}
