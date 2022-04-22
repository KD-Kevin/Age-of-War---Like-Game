#if HE_SYSCORE && STEAMWORKS_NET && HE_STEAMCOMPLETE && !HE_STEAMFOUNDATION && !DISABLESTEAMWORKS 
using UnityEngine;
using UGC = HeathenEngineering.SteamworksIntegration.API.UserGeneratedContent.Client;
using HeathenEngineering.SteamworksIntegration;
using System.Collections.Generic;
using System;
using System.Text;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene7Behaviour : MonoBehaviour
    {
        public UserGeneratedContentQueryManager queryManager;
        public UnityEngine.UI.Text pageCount;
        public UnityEngine.UI.InputField searchInput;
        public GameObject recordTemplate;
        public Transform contentRoot;

        private List<GameObject> currentRecords = new List<GameObject>();

        public void QueryItems()
        {
            queryManager.SearchAll(searchInput.text);
        }

        public void UpdateResults(List<UGCCommunityItem> results)
        {
            pageCount.text = queryManager.activeQuery.Page.ToString() + " of " + queryManager.activeQuery.pageCount.ToString();

            while (currentRecords.Count > 0)
            {
                var target = currentRecords[0];
                currentRecords.Remove(target);
                Destroy(target);
            }

            foreach (var result in results)
            {
                var go = Instantiate(recordTemplate, contentRoot);
                currentRecords.Add(go);
                var comp = go.GetComponent<Scene7DisplayItem>();
                comp.AssignResult(result);
            }
        }

        public void ListSubscribedItems()
        {
            var items = UGC.GetSubscribedItems();
            if (items != null)
            {
                Debug.Log("Found " + items.Length + " items.");
            }
            else
                Debug.Log("Found 0 items.");
        }


        /// <summary>
        /// This method can be used for crude testing of the Create Item method
        /// This is highly dependent on your configuraiton ... for example in Spacewar the max file size is set to 3kb so your image size and folder content would need to be smaller than that to work
        /// Similarly where on your disk 
        /// </summary>
        [ContextMenu("Create Test UGC")]
        public void TestUGCCreate()
        {
            var path = Application.dataPath;
            path = path.Substring(0, path.Length - 7);

            var data = new WorkshopItemData
            {
                appId = SteamworksIntegration.API.App.Client.Id,
                contentFolder = path + "\\TestSpace\\TestUGC",
                description = "description",
                previewImageFile = path + "\\TestSpace\\TestUGC.jfif",
                title = "title",
            };

            var result = SteamworksIntegration.API.UserGeneratedContent.Client.CreateItem(data, (responce) =>
            {
                if (responce.hasError)
                    Debug.LogError(responce.errorMessage);
                else
                    Debug.Log("Created Item");
            });
        }
    }
}
#endif