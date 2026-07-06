using Blazored.LocalStorage;
using Dialysis.Client.Domain.Extensions;
using Dialysis.Shared.Params;
using Dialysis.Shared.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static DevExpress.Data.Helpers.FindSearchRichParser;

namespace Dialysis.Client.Domain.Account
{
    public interface IAuthenticationManager
    {
        Task<IRetResult> Login(LoginParams model);

        Task<IRetResult> Logout();

        Task<ClaimsPrincipal> CurrentUser();
    }

    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
       
        public AuthenticationManager(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            
        }

        public async Task<ClaimsPrincipal> CurrentUser()
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return state.User;
        }

        public async Task<IRetResult> Login(LoginParams model)
        {
            //var response = await _httpClient.PostAsJsonAsync("api/account/token", model);
            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);
            var result = await response.ToResult<LoginResponse>();
            if (result.Succeeded)
            {
                var token = result.Data.Token;
                await _localStorage.SetItemAsync("token", token);
                await SetFioFromJwt(token);

                await ((AppStateProvider)this._authenticationStateProvider).StateChangedAsync();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await Result.SuccessAsync();
            }
            else
            {
                return await Result.FailAsync(result.Messages);
            }
        }

        public async Task<IRetResult> Logout()
        {
            try
            {
                await _httpClient.PostAsync("api/account/logout", null);
            }
            catch
            {
                // Server unreachable — still log out locally
            }

            await _localStorage.RemoveItemAsync("medcenters");
            await _localStorage.RemoveItemAsync("medmachines");
            await _localStorage.RemoveItemAsync("fio");
            await _localStorage.RemoveItemAsync("token");           
            ((AppStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return await Result.SuccessAsync();
        }

        private async Task SetFioFromJwt(string jwt)
        {           
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            string lastName = "";
            string firstName = "";
            if (keyValuePairs != null)
            {
                foreach (var item in keyValuePairs)
                {
                    if (item.Key.ToLower().Contains("surname"))
                    {
                        lastName = item.Value.ToString();
                    }
                    else if (item.Key.ToLower().Contains("nameidentifier"))
                    {
                       
                    }
                    else if (item.Key.ToLower().Contains("givenname"))
                    {
                        
                    }
                    else if (item.Key.ToLower().Contains("name"))
                    {
                        firstName  = item.Value.ToString();
                    }
                }

                await _localStorage.SetItemAsync("fio", firstName+" "+ lastName);
            }
            
        }

        private byte[] ParseBase64WithoutPadding(string payload)
        {
            payload = payload.Trim().Replace('-', '+').Replace('_', '/');
            var base64 = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            return Convert.FromBase64String(base64);
        }
    }
}