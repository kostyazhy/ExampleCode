using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;


/*
 *  The Loader Bundles class allows the player to control the operation of assetbundles.
 * Creates a ui list of asset bundles that are requested in the game API.
 * Information is parsed about asset bundles in a special InfoBundle class.
 * The player can download or delete the selected asset bundle.
 * The corresponding action is performed by clicking the button for each asset bundle separately.
*/

public class LoaderBundles : MonoBehaviour
{
    // use in LoaderContents.cs 
    public delegate void LoadingEvent(InfoBundle infoBundle);
    // launching the event to load the asset bundle
    public static event LoadingEvent BundleLoadingEvent;

    public RectTransform prefabItem;
    public RectTransform containerForItems;
    List<ItemView> updateItemsView;
    InfoBundle[] infoBundles;

    public Sprite loadSpr;
    public Sprite deleteSpr;

    private Color titleTextColor = new Color(0.8396226f, 0.2733728f, 0.1861427f);

    // url API(get) request information about all assets-bundles 
    public string url = "http://92.53.120.203:3012/get-assets-bundles";

    private void OnEnable()
    {
        // After the selected action with asset bundle has occurred, run
        // ChangeItemViewBundle method
        LoaderContents.OnLoaded += ChangeItemViewBundle;
    }

    private void OnDisable()
    {
        LoaderContents.OnLoaded -= ChangeItemViewBundle;
    }

    private void Start()
    {
        CreateItemsBundle();
    }

    // Search for a bundle and change its appearance
    void ChangeItemViewBundle(bool existBundle, string nameBundle)
    {
        foreach (ItemView itemView in updateItemsView) {
            if (itemView.titleText.text == nameBundle) {
                if (existBundle) {
                    itemView.imgButton.sprite = deleteSpr;
                } else {
                    itemView.imgButton.sprite = loadSpr;
                }
                itemView.loaderBar.SetActive(false);
            }
        }
    }

    //requesting information about assets bundles
    public void CreateItemsBundle()
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        StartCoroutine(GetItems(www, results => OnReceivedModels(results)));
    }

    // Getting a list of bundles from the API
    IEnumerator GetItems(UnityWebRequest www, System.Action<InfoBundle[]> callback)
    {
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError) {
            // parsing information about bundles
            infoBundles = JsonHelper.getJsonArray<InfoBundle>(www.downloadHandler.text);
            callback(infoBundles);
        } else {
            InfoBundle[] errList = new InfoBundle[1];
            errList[0] = new InfoBundle {
                nameBundle = "The server is not available\n" + www.error
            };
            callback(errList);
        }
    }

    // Initializing the list of bundles
    void OnReceivedModels(InfoBundle[] infoBundles)
    {
        if (infoBundles == null)
            return;
        foreach (Transform child in containerForItems) {
            Destroy(child.gameObject);
        }

        updateItemsView = new List<ItemView>();
        foreach (var infoBundle in infoBundles) {
            Transform instanceItem = Instantiate(prefabItem);
            instanceItem.SetParent(containerForItems, false);
            InitItemsView(instanceItem, infoBundle);
        }
    }

    // Provide the look and feel initialization
    void InitItemsView(Transform viewGameObj, InfoBundle infoBundle)
    {
        ItemView itemView = new ItemView(viewGameObj);

        if (infoBundle == null) {
            itemView.titleText.text = "Error: bundle empty";
            itemView.imgButton.enabled = false;
            itemView.titleText.color = titleTextColor;
            return;
        }
        itemView.titleText.text = infoBundle.nameBundle;

        if(DataManager.ChackExistFile(infoBundle.nameBundle)) {
            itemView.imgButton.sprite = deleteSpr;
        } else {
            itemView.imgButton.sprite = loadSpr;
        }

        // show an image that the bandel is new
        itemView.newImg.SetActive(infoBundle.newApp);
        // configuring the button to manage the bandel
        itemView.clickButton.onClick.AddListener(
            () =>
            {
                itemView.loaderBar.SetActive(true);
                // launching the event to load the bandle
                BundleLoadingEvent?.Invoke(infoBundle);
            }
            );
        updateItemsView.Add(itemView);
    }

    // class for controlling the appearance of list items bundle
    public class ItemView
    {
        public Text titleText;
        public Button clickButton;
        public Image imgButton;
        public GameObject loaderBar;
        public GameObject newImg;

        public ItemView(Transform rootView)
        {
            titleText = rootView.Find("TitleText").GetComponent<Text>();
            clickButton = rootView.Find("ClickButton").GetComponent<Button>();
            imgButton = rootView.Find("ClickButton").GetComponent<Image>();
            newImg = rootView.Find("NewImg").gameObject;
            loaderBar = rootView.Find("LoaderBar").gameObject;
            loaderBar.SetActive(false);
        }
    }

    // Class for storing information about a bundle
    [System.Serializable]
    public class InfoBundle
    {
        public string idApp;
        public string nameBundle;
        public string urlImgApp;
        public string urlBundle;
        public string nameDataSet;
        public string urlDataSet;
        public string namePrefab;
        public bool newApp;
    }

    // Json parsing
    public class JsonHelper
    {
        public static T[] getJsonArray<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        public static string arrayToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.array = array;
            return JsonUtility.ToJson(wrapper);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
