
public struct RequestInfo
{
        
    public string? LocalIpAddress { get; set; }
    public int LocalPort { get; set; }
    public string? RemoteIpAddress { get; set; }
    public int RemotePort { get; set; }
        
    public string? Protocol { get; set; }
    
    public string? QueryString { get; set; }
    public string? User  { get; set; }

    public string? Host  { get; set; }

    public  string? Headers { get; set; }

    

    public RequestInfo(HttpContext httpContext)
    {
        ConnectionInfo connection = httpContext.Connection;
        if (connection == null)
            return;

        HttpRequest request = httpContext.Request;

        User = httpContext.User.ToString();
        Host = request.Host.Value;
        
        Protocol = request.Protocol;

        QueryString = request.QueryString.Value;

        Headers = "";
        foreach (var headerName in request.Headers.Keys)
        {
            Headers += headerName + ": " + request.Headers[headerName] + " | ";
        }

        httpContext.Request.Headers.ToString();

        LocalPort = connection.LocalPort;
        RemotePort = connection.RemotePort;

        LocalIpAddress = "";
        RemoteIpAddress = "";


        if (connection.LocalIpAddress != null)
            LocalIpAddress = connection.LocalIpAddress.ToString();

        if (connection.RemoteIpAddress != null)
            RemoteIpAddress = connection.RemoteIpAddress.ToString();

    }

}
