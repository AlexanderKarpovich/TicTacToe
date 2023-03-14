namespace TicTacToe.UnitTests.Helpers
{
    public class TestServerCallContext : ServerCallContext
    {
        private readonly Metadata requestHeaders;
        private readonly CancellationToken cancellationToken;
        private readonly Metadata responseTrailers;
        private readonly AuthContext authContext;
        private readonly Dictionary<object, object> userState;
        private WriteOptions? writeOptions;

        private TestServerCallContext(Metadata requestHeaders, CancellationToken cancellationToken)
        {
            this.requestHeaders = requestHeaders;
            this.cancellationToken = cancellationToken;
            responseTrailers = new Metadata();
            authContext = new AuthContext(string.Empty, new Dictionary<string, List<AuthProperty>>());
            userState = new Dictionary<object, object>();
        }

        protected override string MethodCore => "MethodName";
        protected override string HostCore => "HostName";
        protected override string PeerCore => "PeerName";
        protected override DateTime DeadlineCore { get; }
        protected override Metadata RequestHeadersCore => requestHeaders;
        protected override CancellationToken CancellationTokenCore => cancellationToken;
        protected override Metadata ResponseTrailersCore => responseTrailers;
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get => writeOptions; set { writeOptions = value; } }
        protected override AuthContext AuthContextCore => authContext;

        protected override IDictionary<object, object> UserStateCore => userState;

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotImplementedException();
        }

        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            foreach (Metadata.Entry entry in responseHeaders)
            {
                ResponseTrailers.Add(entry);
            }
            return Task.CompletedTask;
        }


        public static TestServerCallContext Create(Metadata? requestHeaders = null, CancellationToken cancellationToken = default)
        {
            return new TestServerCallContext(requestHeaders ?? new Metadata(), cancellationToken);
        }
    }
}