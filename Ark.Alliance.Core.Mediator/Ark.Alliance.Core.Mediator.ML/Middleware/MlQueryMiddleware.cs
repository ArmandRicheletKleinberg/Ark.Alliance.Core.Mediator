using Ark.Alliance.Core.Mediator.Messaging;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;

namespace Ark.Alliance.Core.Mediator.ML;

/// <summary>
/// Captures query payloads for ML training and enqueues them for ingestion.
/// </summary>
public sealed class MlQueryMiddleware<TQuery, TResult> : IQueryMiddleware<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly MlTelemetryOptions _options;
    private readonly ChannelWriter<byte[]> _writer;
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    private readonly Aes _aes;

    public MlQueryMiddleware(MlTelemetryOptions options, Channel<byte[]> queue)
    {
        _options = options;
        _writer = queue.Writer;
        _aes = Aes.Create();
        _aes.Key = Convert.FromBase64String(options.EncryptionKey);
    }

    public async Task<Result<TResult>> HandleAsync(
        TQuery query,
        QueryHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        if (ShouldSample())
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(query);
            var encrypted = Encrypt(payload);
            await _writer.WriteAsync(encrypted, cancellationToken);
        }

        return await next();
    }

    private bool ShouldSample()
    {
        if (_options.SamplingRate <= 0) return false;
        if (_options.SamplingRate >= 100) return true;
        Span<byte> b = stackalloc byte[2];
        _rng.GetBytes(b);
        var value = BitConverter.ToUInt16(b);
        return value % 100 < _options.SamplingRate;
    }

    private byte[] Encrypt(byte[] data)
    {
        _aes.GenerateIV();
        using var enc = _aes.CreateEncryptor();
        var cipher = enc.TransformFinalBlock(data, 0, data.Length);
        var result = new byte[_aes.IV.Length + cipher.Length];
        _aes.IV.CopyTo(result, 0);
        cipher.CopyTo(result, _aes.IV.Length);
        return result;
    }
}
