using System.CodeDom.Compiler;
using SQLitePCL;

namespace WhatDoYouMeme.Api.Services;

public static class JoinCodeGenerator
{
    public static string Generate()
    {
        ReadOnlySpan<char> alphabet = "abcdefghijklmnopqrstuvwxyz";
        
        // Pick 4 random characters from the alphabet span
        char[] randomChars = Random.Shared.GetItems(alphabet, 4);
        
        // Convert the character array to a string
        string randomString = new string(randomChars);

        return randomString.ToUpper();
    }
}