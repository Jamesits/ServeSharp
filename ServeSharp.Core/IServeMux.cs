namespace ServeSharp.Core;
public interface IServeMux<in TContext>
{
    public Middleware.Middleware ServeHttp(TContext context);
}
