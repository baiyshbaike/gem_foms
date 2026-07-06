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
using System.Threading.Tasks;
using Dialysis.Shared.Models;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dialysis.Client.Domain.Common
{
    public interface ICommonService
    {
        Task<List<MedCenter>> LoadRelMedcentersAsync();
        Task<List<MedCenterMachine>> LoadMedcenterMachinesAsync(long medcenterId);

        Task<List<District>> LoadDistrictsAsync();
        Task<List<Region>> LoadRegionsAsync(long distId);
        Task<List<Region>> LoadAllRegionsAsync();

        Task<int> LocalMedCardCleanAllAsync();
        Task<MedCard> LocalMedCard0Async(MedCard? entity);
        Task<FirstInspection> LocalMedCard1Async(FirstInspection? entity);
        Task<FirstRespiratory> LocalMedCard2Async(FirstRespiratory? entity);
        Task<FirstCardiovascular> LocalMedCard3Async(FirstCardiovascular? entity);
        Task<FirstConfectionery> LocalMedCard4Async(FirstConfectionery? entity);
        Task<FirstUrogenital> LocalMedCard5Async(FirstUrogenital? entity);
        Task<FirstEndocrine> LocalMedCard6Async(FirstEndocrine? entity);
        Task<FirstNeuro> LocalMedCard7Async(FirstNeuro? entity);
    }

    public class CommonService : ICommonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public CommonService(
            HttpClient httpClient,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }
        
        public async Task<List<MedCenter>> LoadRelMedcentersAsync()
        {
            var isLocal = false;
            var localCenters = await _localStorage.GetItemAsync<List<MedCenter>>("medcenters");
            if (localCenters != null)
            {
                if (localCenters.Count > 0)
                {
                    isLocal = true;
                }
            }

            if (isLocal == false)
            {
                await _localStorage.RemoveItemAsync("medcenters");           
                var response = await _httpClient.GetAsync("api/medcenter/relmedcenters");
                var mappedData = await response.ToResult<List<MedCenter>>();
                var UserCenters = mappedData.Data;
                await _localStorage.SetItemAsync("medcenters", UserCenters);
                return UserCenters;
            }
            else
            {
                return localCenters;
            }
        }

        public async Task<List<MedCenterMachine>> LoadMedcenterMachinesAsync(long medcenterId)
        {
            var isLocal = false;
            var localMachines = await _localStorage.GetItemAsync<List<MedCenterMachine>>("medmachines");
            var centerMachines = new List<MedCenterMachine>();
            if (localMachines != null)
            {
                if (localMachines.Count > 0)
                {
                    centerMachines = localMachines.Where(p => p.MedCenterId.Equals(medcenterId)).ToList();
                    if (centerMachines != null)
                    {
                        if (centerMachines.Count > 0)
                        {
                            isLocal = true;
                        }
                    }
                }
            }
            if (isLocal == false)
            {
                await _localStorage.RemoveItemAsync("medmachines");        
                var response = await _httpClient.GetAsync("api/medcenter/allmachines?Id=" + medcenterId);
                var mappedData = await response.ToResult<List<MedCenterMachine>>();
                var MachineList = mappedData.Data;
                if (MachineList != null)
                {
                    localMachines.AddRange(MachineList);
                    await _localStorage.SetItemAsync("medmachines", localMachines);
                }
                return MachineList;
            }
            else
            {
                return centerMachines;
            }
        }

        public async Task<List<District>> LoadDistrictsAsync()
        {
            var isLocal = false;
            var localCenters = await _localStorage.GetItemAsync<List<District>>("districts");
            if (localCenters != null)
            {
                if (localCenters.Count > 0)
                {
                    isLocal = true;
                }
            }

            if (isLocal == false)
            {
                await _localStorage.RemoveItemAsync("districts");
                var response = await _httpClient.GetAsync("api/region/alldistricts");
                var mappedData = await response.ToResult<List<District>>();
                var UserCenters = mappedData.Data;
                await _localStorage.SetItemAsync("districts", UserCenters);
                return UserCenters;
            }
            else
            {
                return localCenters;
            }
        }

        public async Task<List<Region>> LoadRegionsAsync(long distId)
        {
            var isLocal = false;
            var localMachines = await _localStorage.GetItemAsync<List<Region>>("allregions");
            var centerMachines = new List<Region>();
            if (localMachines != null)
            {
                if (localMachines.Count > 0)
                {
                    centerMachines = localMachines.Where(p => p.ParentId.Equals(distId)).ToList();
                    if (centerMachines != null)
                    {
                        if (centerMachines.Count > 0)
                        {
                            isLocal = true;
                        }
                    }
                }
            }
            if (isLocal == false)
            {
                await _localStorage.RemoveItemAsync("allregions");
                var response = await _httpClient.GetAsync("api/region/alldistricts");
                var mappedData = await response.ToResult<List<Region>>();
                var MachineList = mappedData.Data;
                if (MachineList != null)
                {
                    //localMachines.AddRange(MachineList);
                    await _localStorage.SetItemAsync("allregions", MachineList);

                    centerMachines = MachineList.Where(p => p.ParentId.Equals(distId)).ToList();                    
                }               
            }
            
            return centerMachines;
            
        }

        public async Task<List<Region>> LoadAllRegionsAsync()
        {
            var isLocal = false;
            var centerMachines = await _localStorage.GetItemAsync<List<Region>>("allregions");
            if (centerMachines != null)
            {
                if (centerMachines.Count > 0)
                {
                    isLocal = true;                       
                }
            }
            if (isLocal == false)
            {
                await _localStorage.RemoveItemAsync("allregions");
                var response = await _httpClient.GetAsync("api/region/allregions");
                var mappedData = await response.ToResult<List<Region>>();
                centerMachines = mappedData.Data;
                if (centerMachines != null)
                {
                    await _localStorage.SetItemAsync("allregions", centerMachines);                  
                }
            }
            return centerMachines;
        }

        public async Task<int> LocalMedCardCleanAllAsync()
        {            
            await _localStorage.RemoveItemAsync("medcard0");
            await _localStorage.RemoveItemAsync("medcard1");
            await _localStorage.RemoveItemAsync("medcard2");
            await _localStorage.RemoveItemAsync("medcard3");
            await _localStorage.RemoveItemAsync("medcard4");
            await _localStorage.RemoveItemAsync("medcard5");
            await _localStorage.RemoveItemAsync("medcard6");
            await _localStorage.RemoveItemAsync("medcard7");
            return 0;
        }

        public async Task<MedCard> LocalMedCard0Async(MedCard? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard0");
                await _localStorage.SetItemAsync("medcard0", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<MedCard>("medcard0");
                return localItem;
            }           
        }

        public async Task<FirstInspection> LocalMedCard1Async(FirstInspection? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard1");
                await _localStorage.SetItemAsync("medcard1", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstInspection>("medcard1");
                return localItem;
            }
        }
        public async Task<FirstRespiratory> LocalMedCard2Async(FirstRespiratory? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard2");
                await _localStorage.SetItemAsync("medcard2", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstRespiratory>("medcard2");
                return localItem;
            }
        }
        public async Task<FirstCardiovascular> LocalMedCard3Async(FirstCardiovascular? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard3");
                await _localStorage.SetItemAsync("medcard3", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstCardiovascular>("medcard3");
                return localItem;
            }
        }
        public async Task<FirstConfectionery> LocalMedCard4Async(FirstConfectionery? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard4");
                await _localStorage.SetItemAsync("medcard4", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstConfectionery>("medcard4");
                return localItem;
            }
        }
        public async Task<FirstUrogenital> LocalMedCard5Async(FirstUrogenital? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard5");
                await _localStorage.SetItemAsync("medcard5", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstUrogenital>("medcard5");
                return localItem;
            }
        }
        public async Task<FirstEndocrine> LocalMedCard6Async(FirstEndocrine? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard6");
                await _localStorage.SetItemAsync("medcard6", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstEndocrine>("medcard6");
                return localItem;
            }
        }
        public async Task<FirstNeuro> LocalMedCard7Async(FirstNeuro? entity)
        {
            if (entity != null)
            {
                await _localStorage.RemoveItemAsync("medcard7");
                await _localStorage.SetItemAsync("medcard7", entity);
                return entity;
            }
            else
            {
                var localItem = await _localStorage.GetItemAsync<FirstNeuro>("medcard7");
                return localItem;
            }
        }



    }
}