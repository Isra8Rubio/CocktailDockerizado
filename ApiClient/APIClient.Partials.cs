using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.APIService
{
    public partial class APIClient
    {
        private static string _token;
        public static string Token
        {
            get => _token;
            set
            {
                if (_token == value) return;
                _token = value;
                TokenChanged?.Invoke();   // ← notifica cambios para que no haga cosas raras cuando F5 o Cambio idioma
            }
        }

        public static event Action TokenChanged;

        private static void AttachBearer(HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        // Hooks
        protected virtual Task PrepareRequestAsync(HttpClient c, HttpRequestMessage r, StringBuilder u, CancellationToken ct)
        { AttachBearer(r); return Task.CompletedTask; }

        protected virtual Task PrepareRequestAsync(HttpClient c, HttpRequestMessage r, string u, CancellationToken ct)
        { AttachBearer(r); return Task.CompletedTask; }

        protected virtual Task ProcessResponseAsync(HttpClient c, HttpResponseMessage r, CancellationToken ct)
            => Task.CompletedTask;

        protected virtual void PrepareRequest(HttpClient c, HttpRequestMessage r, StringBuilder u)
            => AttachBearer(r);

        protected virtual void PrepareRequest(HttpClient c, HttpRequestMessage r, string u)
            => AttachBearer(r);

        protected virtual void ProcessResponse(HttpClient c, HttpResponseMessage r) { }
    }
}
