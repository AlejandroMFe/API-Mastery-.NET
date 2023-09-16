namespace API.FurnitureStore.Shared.Auth;
public class AuthResult
{
    public string Token { get; set; }
    public bool Result { get; set; }
    public IEnumerable<string> Errors { get; set; }
}
