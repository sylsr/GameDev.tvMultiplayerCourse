using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;

    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        if(AuthState == AuthState.Authenticating)
        {
            Debug.LogWarning("Authentication already in process");
            await (AuthenticatingAsync());
        }

        await SignInAnonymouslyAsync(maxTries);

        return AuthState;
    }

    private static async Task<AuthState> AuthenticatingAsync()
    {
        while(AuthState == AuthState.Authenticating || AuthState == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxTries)
    {
        AuthState = AuthState.Authenticating;
        for (int i = 0; i < maxTries && AuthState == AuthState.Authenticating; i++)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                {
                    AuthState = AuthState.Authenticated;
                    break;
                }

                await Task.Delay(1000);
            }
            catch (AuthenticationException e)
            {
                Debug.LogError(e.Message);
                AuthState = AuthState.Error;
            }
            catch (RequestFailedException e)
            {
                Debug.LogError(e.Message);
                AuthState = AuthState.Error;
            }
        }

        if(AuthState != AuthState.Authenticated)
        {
            Debug.LogWarning($"Player was not signed in successfully after {maxTries} tries");
            AuthState = AuthState.TimeOut;
        }
    }

}



public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}
