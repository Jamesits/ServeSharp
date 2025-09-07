using System.Threading.Tasks;

namespace ServeSharp.Core;
public interface IServeMux<in TContext>
{
    public Task ServeHttp(TContext context);
}
