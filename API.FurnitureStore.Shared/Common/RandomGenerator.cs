namespace API.FurnitureStore.Shared.Common;
public static class RandomGenerator
{
    public static string GenerateRandomString(int size)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz$%&._-+)=?";

        return new string(Enumerable.Repeat(chars, size)
                                    .Select(c => c[random.Next(c.Length)])
                                    .ToArray());
    }
}
