namespace ElMaAPI;

public class LoginResponse
{
    public LoginResponse()
    {
        Token = string.Empty;
        ResponseMsg = new HttpResponseMessage()
        {
            StatusCode = System.Net.HttpStatusCode.Unauthorized
        };
    }

    public string Token { get; set; }
    public HttpResponseMessage ResponseMsg { get; set; }
}