﻿namespace API.FurnitureStore.Shared.DTOs;
public class TokenRequest
{
    [Required]
    public string Token { get; set; }
    [Required]
    public string RefreshToken { get; set; }
}
