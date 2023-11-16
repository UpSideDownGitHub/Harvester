using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using MasterServerToolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace MasterServerToolkit.Bridges
{
    public class MapListView : UIView
    {
        [Header("Components"), SerializeField]
        private UILable uiLablePrefab;
        [SerializeField]
        private UILable uiColLablePrefab;
        [SerializeField]
        private UIButton uiButtonPrefab;
        [SerializeField]
        private RectTransform listContainer;
        [SerializeField]
        private TMP_Text statusInfoText;

        public PickedData pickedData;

        public PlayerListView playerList;

        protected override void Awake()
        {
            base.Awake();

            // Listen to show/hide events
            Mst.Events.AddListener(MstEventKeys.showGamesListView, OnShowMapsListEventHandler);
            Mst.Events.AddListener(MstEventKeys.hideGamesListView, OnHideMapsListEventHandler);
        }

        protected override void Start()
        {
            base.Start();

            if (listContainer)
            {
                foreach (Transform t in listContainer)
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void OnShowMapsListEventHandler(EventMessage message)
        {
            Show();
        }

        private void OnHideMapsListEventHandler(EventMessage message)
        {
            Hide();
        }

        protected override void OnShow()
        {
            FindMaps();
        }

        private void DrawMapsList(MapSaveData mapData)
        {
            if (listContainer)
            {
                int index = 0;

                var gameNumberCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameNumberCol.Text = "#";
                gameNumberCol.name = "gameNumberCol";

                var gameNameCol = Instantiate(uiColLablePrefab, listContainer, false);
                gameNameCol.Text = "Name";
                gameNameCol.name = "gameNameCol";

                var connectBtnCol = Instantiate(uiColLablePrefab, listContainer, false);
                connectBtnCol.Text = "#";
                connectBtnCol.name = "connectBtnCol";

                foreach (MapData map in mapData.maps)
                {
                    var gameNumberLable = Instantiate(uiLablePrefab, listContainer, false);
                    gameNumberLable.Text = $"{index + 1}";
                    gameNumberLable.name = $"gameNumberLable_{index}";

                    var gameNameLable = Instantiate(uiLablePrefab, listContainer, false);
                    gameNameLable.Text = map.mapName.ToString();
                    gameNameLable.name = $"gameNameLable_{index}";

                    var gameConnectBtn = Instantiate(uiButtonPrefab, listContainer, false);
                    gameConnectBtn.SetLable("Select");
                    gameConnectBtn.SetID(index);
                    Debug.Log("INDEX AT CREATION: " + index);
                    gameConnectBtn.AddOnClickListener(() =>
                    {
                        pickedData.mapID = gameConnectBtn.GetID();
                        Debug.Log("INDEXT AT PRINTING: " + gameConnectBtn.GetID());
                        playerList.Show();
                        Hide();
                    });
                    gameConnectBtn.name = $"gameConnectBtn_{index}";

                    index++;
                }
            }
            else
                logger.Error("Not all components are setup");
        }

        private void ClearMapsList()
        {
            if (listContainer)
            {
                foreach (Transform tr in listContainer)
                {
                    Destroy(tr.gameObject);
                }
            }
        }

        public void FindMaps()
        {
            ClearMapsList();
            if (statusInfoText)
            {
                statusInfoText.text = "Finding maps... Please wait!";
                statusInfoText.gameObject.SetActive(true);
            }
            MapSaveData data = SaveManager.instance.LoadMapSaveData();
            if (data.maps.Count == 0)
            {
                statusInfoText.text = "No maps found! Try to create one.";
                return;
            }
            statusInfoText.gameObject.SetActive(false);
            DrawMapsList(data);
        }
    }
}