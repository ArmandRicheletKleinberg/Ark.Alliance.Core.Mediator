# Copier

Follow these steps to reuse the streaming sample as a starting point for other projects:

1. Copy the `StreamingLoggingSample` folder to your solution's `Samples` directory.
2. Update the project references in `StreamingLoggingSample.csproj` if your folder layout differs.
3. Run `dotnet restore` and `dotnet run` to verify the sample still builds and streams data.
4. Edit `Program.cs` to register your own handlers and middleware as required.
5. Build a container image with `docker build -t streaming-sample .` for portable execution.
