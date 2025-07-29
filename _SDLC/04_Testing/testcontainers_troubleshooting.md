# Testcontainers Troubleshooting

Common issues when running integration tests with Testcontainers.

## Docker Not Running
- Ensure Docker Desktop or the Docker daemon is started.
- Linux users may need to add their user to the `docker` group.

## Port Conflicts
- Containers expose ports that may already be in use. Stop other services or
  change the port bindings in the test setup.

## Unable to Pull Images
- Verify network connectivity and that the image registry is reachable.
- In CI environments make sure the runner has internet access or a cached image.

## Container Start Failures
- Inspect the output logs printed during `StartAsync`. They often contain the
  root cause such as invalid configuration.
- Clean up dangling containers with `docker ps -a` and `docker rm` if necessary.

For more details refer to the main guide in
[`testcontainers_dotnet_9.Md`](testcontainers_dotnet_9.Md).
See also the [Troubleshooting Procedure](testcontainers_troubleshooting_procedure.md) for a step-by-step checklist.
