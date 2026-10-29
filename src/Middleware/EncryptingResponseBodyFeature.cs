using System.Buffers;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Http.Features;

namespace MariesWonderland.Middleware;

/// <summary>
/// Stands in for the response body while a gRPC call runs: the gRPC server writes plain messages to it, and each
/// complete frame is encrypted into the real body when the server flushes. Starting the response and everything else
/// goes straight to the real body, so headers and trailers behave as usual.
/// </summary>
internal sealed class EncryptingResponseBodyFeature : IHttpResponseBodyFeature
{
    private readonly IHttpResponseBodyFeature _inner;
    private readonly FramePipeWriter _writer;

    public EncryptingResponseBodyFeature(IHttpResponseBodyFeature inner)
    {
        _inner = inner;
        _writer = new FramePipeWriter(inner.Writer);
    }

    public Stream Stream => _writer.AsStream();

    public PipeWriter Writer => _writer;

    /// <summary>Encrypts and writes the frames still waiting, without flushing.</summary>
    public void Transfer() => _writer.Transfer();

    public void DisableBuffering() => _inner.DisableBuffering();

    public Task StartAsync(CancellationToken cancellationToken = default) => _inner.StartAsync(cancellationToken);

    public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellationToken = default)
        => _inner.SendFileAsync(path, offset, count, cancellationToken);

    public Task CompleteAsync()
    {
        _writer.Transfer();
        return _inner.CompleteAsync();
    }

    private sealed class FramePipeWriter(PipeWriter inner) : PipeWriter
    {
        private readonly ArrayBufferWriter<byte> _buffer = new();

        public override void Advance(int bytes) => _buffer.Advance(bytes);

        public override Memory<byte> GetMemory(int sizeHint = 0) => _buffer.GetMemory(sizeHint);

        public override Span<byte> GetSpan(int sizeHint = 0) => _buffer.GetSpan(sizeHint);

        public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            Transfer();
            return inner.FlushAsync(cancellationToken);
        }

        public override void CancelPendingFlush() => inner.CancelPendingFlush();

        public override void Complete(Exception? exception = null)
        {
        }

        public void Transfer()
        {
            int consumed = GrpcPayloadCipher.TransformFrames(_buffer.WrittenSpan, inner, encrypt: true);
            if (consumed == 0)
            {
                return;
            }

            byte[] rest = _buffer.WrittenSpan[consumed..].ToArray();
            _buffer.Clear();
            _buffer.Write(rest);
        }
    }
}
