# Testcontainers Troubleshooting Procedure

When integration tests fail to start their containers, follow these steps to quickly diagnose the issue.

1. **Verify Docker is running**
   - On Windows/macOS ensure Docker Desktop is started.
   - On Linux run `systemctl status docker` and start the service if needed.
2. **Check container logs**
   - The test output prints container logs when startup fails. Review them for configuration errors.
3. **Inspect running containers**
   - Run `docker ps -a` to see lingering containers that may block ports.
   - Remove them with `docker rm <id>` if they are leftovers from previous runs.
4. **Validate image availability**
   - Execute `docker pull rabbitmq:3-management` (or the relevant image) to confirm network access.
5. **Consult the documentation**
   - See `testcontainers_dotnet_9.Md` for supported environments and CI setup instructions.

If issues persist open an issue with the container logs and environment details.
